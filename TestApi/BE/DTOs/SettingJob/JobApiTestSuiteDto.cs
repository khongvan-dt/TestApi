namespace AutoApiTester.DTOs.SettingJob
{
    public class JobApiTestSuiteDto
    {
        public int? Id { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string Headers { get; set; }
        public object DataBase { get; set; }    
        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<JobApiTestCaseDto> TestCases { get; set; } = new();
    }

    public class JobApiTestCaseDto
    {
        public int? Id { get; set; }
        public string? CaseName { get; set; }
        public object? TestData { get; set; }  
        public int ExpectedStatus { get; set; } = 200;
    }

}

