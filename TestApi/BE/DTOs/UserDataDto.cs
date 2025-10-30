namespace AutoApiTester.Models.DTOs
{
    // User DTO
    public class UserDataDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

   
    // ✅ Request DTO theo DB
    public class RequestDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? AuthType { get; set; }
        public string? AuthValue { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ Nested
        public List<RequestParamDto> QueryParams { get; set; } = new();
        public List<RequestHeaderDto> Headers { get; set; } = new();
        public RequestBodyDto? Body { get; set; }
        public string? DataBaseTest { get; set; }

    }

    // Nested child DTOs
    public class RequestParamDto
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    public class RequestHeaderDto
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    public class RequestBodyDto
    {
        public string? BodyType { get; set; }
        public string? Content { get; set; }
    }

    // ✅ Collection DTO theo DB
    public class CollectionDataDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<RequestDataDto> Requests { get; set; } = new();
    }

    // ✅ Main Export DTO
    public class UserDataExportDto
    {
        public UserDataDto? User { get; set; }
        public List<CollectionDataDto> Collections { get; set; } = new();
        public DataSummaryDto Summary { get; set; } = new();
    }

    public class DataSummaryDto
    {
        public int TotalCollections { get; set; }
        public int TotalRequests { get; set; }
     }
    public class ImportResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int ImportedCollections { get; set; }
        public int UpdatedCollections { get; set; }
        public int ImportedRequests { get; set; }
        public int UpdatedRequests { get; set; }
        public int TotalProcessed { get; set; }
    }



    public class SaveRequestDto
    {
        public int? RequestId { get; set; } // Null = create new, có giá trị = update
        public int CollectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = string.Empty;
        public string? AuthType { get; set; }
        public string? AuthValue { get; set; }

        public List<ParamDto> QueryParams { get; set; } = new();
        public List<HeaderDto> Headers { get; set; } = new();
        public BodyDto? Body { get; set; }
    }

    public class ParamDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class HeaderDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class BodyDto
    {
        public int Id { get; set; } 
        public string BodyType { get; set; } = "raw";
        public string Content { get; set; } = string.Empty;
    }

    public class SaveRequestResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int RequestId { get; set; }
        public bool IsNew { get; set; } // true = created, false = updated
    }

}
