Testing & TDD – מדריך למפתחים 

מטרת מסמך זה היא להציג עקרונות עבודה נכונים עם בדיקות אוטומטיות ו־פיתוח מונחה בדיקות (TDD), בצורה פשוטה, מעשית והדרגתית.

למה בכלל בדיקות?

בדיקות אוטומטיות עוזרות לנו:

לוודא שהקוד עובד כמו שציפינו

למנוע שבירת קוד קיים כשמוסיפים פיצ’רים

לשפר את עיצוב הקוד

לעבוד בביטחון גם במערכות גדולות

קוד בלי בדיקות = קוד שחוששים לגעת בו

מה זה TDD?

        TDD – Test Driven Development
שיטת פיתוח שבה כותבים בדיקה לפני שכותבים את הקוד עצמו.

מחזור העבודה (הבסיס של TDD)

        Red – כותבים בדיקה שנכשלת

        Green – כותבים קוד מינימלי שגורם לבדיקה לעבור

        Refactor – משפרים את הקוד בלי לשנות התנהגות

חוזרים על המחזור שוב ושוב, בצעדים קטנים.

איך נראית בדיקה טובה?
מבנה מומלץ: Arrange – Act – Assert
    
        Arrange – הכנת נתונים ותלויות
        Act     – הרצת הקוד הנבדק
        Assert  – בדיקה שהתוצאה נכונה

עקרונות חשובים

בדיקה אחת = התנהגות אחת

הבדיקה צריכה להיות ברורה לקריאה

שם הבדיקה צריך להסביר מה אמור לקרות

דוגמה לשם טוב:

        Should_Return_Error_When_User_Not_Found

Unit Tests – בדיקות יחידה
מה זה?

בדיקות של פונקציה / מחלקה אחת, בלי תלות חיצונית.

מאפיינים

רצות מהר מאוד

לא ניגשות ל־DB, קבצים או רשת

קלות לתחזוקה

כללים למתחילים:

בודקים לוגיקה, לא Framework

משתמשים ב־Mock רק כשיש תלות חיצונית

לא בודקים מימוש פנימי – רק תוצאה

        Integration Tests – בדיקות אינטגרציה
מה זה?

בדיקות שבודקות חיבור בין רכיבים:

        API + DB

        Service + Repository

קוד מול תשתית

חשוב לדעת

יש פחות בדיקות כאלה מ־Unit Tests

הן איטיות יותר

בודקים רק תרחישים חשובים

פירמידת הבדיקות
           End to End (מעט מאוד)
           Integration Tests
          Unit Tests (הרוב)


המטרה:
הרבה בדיקות קטנות ומהירות, מעט בדיקות כבדות.

עקרונות עיצוב שעוזרים לבדיקה

   Dependency Injection – לא ליצור תלויות בתוך המחלקה

עבודה מול Interfaces

מחלקה אחת = אחריות אחת

אם קשה לבדוק קוד – כנראה שהעיצוב לא טוב

טעויות נפוצות של מתחילים

- כתיבת בדיקות רק בשביל Coverage
- בדיקות שתלויות אחת בשנייה
- בדיקות ארוכות ומסובכות
- בדיקת לוגים במקום בדיקת תוצאה

מתי כן חייבים בדיקה?

כתבו בדיקה כאשר:

יש לוגיקה עסקית

יש תנאים / חישובים

יש סיכוי לשינויים עתידיים

תקלה כאן תהיה יקרה

טיפ חשוב לסיום

בדיקות הן לא תוספת –
בדיקות הן חלק מהקוד.

מפתח טוב:

כותב קוד שעובד

מפתח מקצועי כותב קוד שעובד וגם נבדק

how to use it:

1. Create the Test Project

   From the solution folder:

        dotnet new xunit -n MyApp.Tests
        dotnet sln add MyApp.Tests

   Add reference to the project you want to test:

        dotnet add MyApp.Tests reference MyApp.Core

2. Install Required NuGet Packages

   Mandatory
   
        dotnet add package Microsoft.NET.Test.Sdk
        dotnet add package xunit
        dotnet add package xunit.runner.visualstudio

    Mocking
  
        dotnet add package Moq

    Better assertions
  
        dotnet add package FluentAssertions

    (Optional) ASP.NET Core integration tests

        dotnet add package Microsoft.AspNetCore.Mvc.Testing

3. Recommended Folder Structure
 
        MyApp.Tests
         ├── Unit
         │    ├── Services
         │    │    └── OrderServiceTests.cs
         │    └── Helpers
         ├── Integration
         │    └── OrdersControllerTests.cs
         ├── TestBase.cs
         └── MyApp.Tests.csproj

4. First Unit Test (Simple Example)

    Production code (MyApp.Core)

        public class Calculator
        {
            public int Add(int a, int b) => a + b;
        }
        
        Test
        public class CalculatorTests
        {
            [Fact]
            public void Add_WhenCalled_ReturnsSum()
            {
                // Arrange
                var calculator = new Calculator();
        
                // Act
                var result = calculator.Add(2, 3);
        
                // Assert
                result.Should().Be(5);
            }
        }

5. Testing a Service with Dependencies (Moq)

    Production code:
  
                public interface IUserRepository
                {
                    User GetById(int id);
                }
                
                public class UserService
                {
                    private readonly IUserRepository _repo;
                
                    public UserService(IUserRepository repo)
                    {
                        _repo = repo;
                    }
                
                    public string GetUserName(int id)
                    {
                        return _repo.GetById(id)?.Name;
                    }
                }

    Test with Moq:

        public class UserServiceTests
        {
            private readonly Mock<IUserRepository> _repoMock;
            private readonly UserService _service;
        
            public UserServiceTests()
            {
                _repoMock = new Mock<IUserRepository>();
                _service = new UserService(_repoMock.Object);
            }
        
            [Fact]
            public void GetUserName_UserExists_ReturnsName()
            {
                // Arrange
                _repoMock.Setup(r => r.GetById(1))
                         .Returns(new User { Name = "Rachel" });
        
                // Act
                var name = _service.GetUserName(1);
        
                // Assert
                name.Should().Be("Rachel");
            }
        }

6. Parameterized Tests (Theory)
   
        [Theory]
        [InlineData(2, 3, 5)]
        [InlineData(0, 0, 0)]
        [InlineData(-1, 1, 0)]
        public void Add_MultipleInputs_ReturnsCorrectSum(
            int a, int b, int expected)
        {
            var calc = new Calculator();
            calc.Add(a, b).Should().Be(expected);
        }

7. Integration Test (Web API)

     API Test:
   
        public class OrdersControllerTests 
            : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly HttpClient _client;
        
            public OrdersControllerTests(
                WebApplicationFactory<Program> factory)
            {
                _client = factory.CreateClient();
            }
        
            [Fact]
            public async Task GetOrders_Returns200()
            {
                var response = await _client.GetAsync("/api/orders");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

9. Test Naming Convention (Very Important)

  Pattern

        MethodName_StateUnderTest_ExpectedBehavior


 Example:

        GetUserName_UserExists_ReturnsName


   This is industry standard.

10. Common Test Anti-Patterns (Avoid)

        ❌ Testing private methods
        ❌ Using real DB in unit tests
        ❌ Too many asserts in one test
        ❌ Logic inside tests
        ❌ One test testing multiple behaviors

10. Run Tests

             dotnet test

    With coverage:

        dotnet test --collect:"XPlat Code Coverage"

12. Golden Rules (Memorize These)

        ✔ One test = one behavior
        ✔ Unit tests = no IO, no DB, no HTTP
        ✔ Integration tests = real pipeline
        ✔ Tests must be fast
        ✔ Tests must be deterministic
