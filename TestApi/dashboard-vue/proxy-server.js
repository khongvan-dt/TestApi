// proxy-server.js
import express from 'express'
import cors from 'cors'
import axios from 'axios'

const app = express()
const PORT = 3001

// Enable CORS
app.use(cors())
app.use(express.json())

// Health check
app.get('/', (req, res) => {
  res.json({ status: 'Proxy server is running', port: PORT })
})

// Proxy endpoint
app.all('/proxy', async (req, res) => {
  try {
    const targetUrl = req.headers['x-target-url']

    if (!targetUrl) {
      return res.status(400).json({ error: 'Missing x-target-url header' })
    }


    // Build request config
    const config = {
      method: req.method,
      url: targetUrl,
      headers: {},
      timeout: 30000
    }

    // Copy headers (except proxy-specific ones)
    Object.keys(req.headers).forEach(key => {
      if (!['host', 'connection', 'x-target-url', 'content-length'].includes(key.toLowerCase())) {
        config.headers[key] = req.headers[key]
      }
    })

    // Add body for POST/PUT/PATCH
    if (['POST', 'PUT', 'PATCH'].includes(req.method)) {
      config.data = req.body
    }

 
    // Make request
    const response = await axios(config)

 
    // Return response
    res.status(response.status).json(response.data)

  } catch (error) {
 
    if (error.response) {
      res.status(error.response.status).json(error.response.data)
    } else {
      res.status(500).json({
        error: error.message,
        details: 'Proxy server error'
      })
    }
  }
})

 