namespace AutoApiTester.DTOs.SettingJob
{
    public class JobScheduleDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ScheduleType { get; set; } // "daily" | "interval"
        public string DailyTime { get; set; }   // Dùng string "HH:MM" từ UI
        public int? IntervalValue { get; set; } // Giá trị số
        public string? IntervalUnit { get; set; } // "minutes" | "hours"
        public List<JobApiTestSuiteDto> TestSuites { get; set; }
    }
    public class JobApiTestSuiteDto
    {
        public int Id { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public object Headers { get; set; }
        public object DataBase { get; set; }    
         public string? Description { get; set; }

        public List<JobApiTestCaseDto> TestCases { get; set; } = new();
    }

    public class JobApiTestCaseDto
    {
        public int Id { get; set; }
        public string? CaseName { get; set; }
        public object? TestData { get; set; }  
        public int ExpectedStatus { get; set; } = 200;
    }

}

