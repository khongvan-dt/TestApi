namespace AutoApiTester.Models.DTOs
{
    // ====================================
    // 🔴 KHÔNG THAY ĐỔI - User DTO
    // ====================================
    public class UserDataDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    // ====================================
    // ✅ THAY ĐỔI - Request DTO (Body → Bodies)
    // ====================================
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

        // ✅ THAY ĐỔI: Body → Bodies (List)
        public List<RequestBodyDto> Bodies { get; set; } = new();

        public string? DataBaseTest { get; set; }
    }

    // ====================================
    // 🔴 KHÔNG THAY ĐỔI - Nested child DTOs
    // ====================================
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
        public int Id { get; set; }
        public string? BodyType { get; set; }
        public string? Value { get; set; }
        public string? Type { get; set; }

    }

    // ====================================
    // 🔴 KHÔNG THAY ĐỔI - Collection DTO
    // ====================================
    public class CollectionDataDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<RequestDataDto> Requests { get; set; } = new();
    }

    // ====================================
    // 🔴 KHÔNG THAY ĐỔI - Main Export DTO
    // ====================================
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

    // ====================================
    // 🔴 KHÔNG THAY ĐỔI - Import Result DTO
    // ====================================
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

    // ====================================
    // 🔴 KHÔNG THAY ĐỔI - Save Request DTOs
    // ====================================
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
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

    }

    public class SaveRequestResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int RequestId { get; set; }
        public bool IsNew { get; set; } // true = created, false = updated
    }
}