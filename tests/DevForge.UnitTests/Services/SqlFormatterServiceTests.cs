using FluentAssertions;
using DevForge.Application.Common.Models;
using DevForge.Application.Services;

namespace DevForge.UnitTests.Services;

public class SqlFormatterServiceTests
{
    private readonly SqlFormatterService _service;

    public SqlFormatterServiceTests()
    {
        _service = new SqlFormatterService();
    }

    [Fact]
    public void Format_WithEmptyInput_Should_ReturnEmptyString()
    {
        // Arrange
        var request = new SqlFormatterRequest { Sql = "" };

        // Act
        var response = _service.Format(request);

        // Assert
        response.FormattedSql.Should().BeEmpty();
        response.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Format_WithSelectQuery_Should_FormatAndUppercaseKeywords()
    {
        // Arrange
        var request = new SqlFormatterRequest 
        { 
            Sql = "select id,name,email from users where isactive=1 order by name" 
        };

        // Act
        var response = _service.Format(request);

        // Assert
        response.IsValid.Should().BeTrue();
        response.FormattedSql.Should().Contain("SELECT");
        response.FormattedSql.Should().Contain("FROM");
        response.FormattedSql.Should().Contain("WHERE");
        response.FormattedSql.Should().Contain("ORDER BY");
        response.FormattedSql.Should().ContainEquivalentOf("isactive = 1");
    }

    [Fact]
    public void Format_WithJoinAndGroupBy_Should_FormatCorrectly()
    {
        // Arrange
        var request = new SqlFormatterRequest
        {
            Sql = "select o.id, count(i.id) as itemcount from orders o join orderitems i on o.id = i.orderid group by o.id"
        };

        // Act
        var response = _service.Format(request);

        // Assert
        response.IsValid.Should().BeTrue();
        response.FormattedSql.Should().Contain("JOIN");
        response.FormattedSql.Should().Contain("ON");
        response.FormattedSql.Should().Contain("GROUP BY");
    }

    [Fact]
    public void Format_WithInsertUpdateDelete_Should_FormatSuccessfully()
    {
        // Arrange
        var insertReq = new SqlFormatterRequest { Sql = "insert into users (name,email) values ('alice','alice@test.com')" };
        var updateReq = new SqlFormatterRequest { Sql = "update users set name='bob' where id=5" };
        var deleteReq = new SqlFormatterRequest { Sql = "delete from users where active=0" };

        // Act
        var insertRes = _service.Format(insertReq);
        var updateRes = _service.Format(updateReq);
        var deleteRes = _service.Format(deleteReq);

        // Assert
        insertRes.IsValid.Should().BeTrue();
        insertRes.FormattedSql.Should().Contain("INSERT");
        insertRes.FormattedSql.Should().Contain("INTO");
        insertRes.FormattedSql.Should().Contain("VALUES");

        updateRes.IsValid.Should().BeTrue();
        updateRes.FormattedSql.Should().Contain("UPDATE");
        updateRes.FormattedSql.Should().Contain("SET");

        deleteRes.IsValid.Should().BeTrue();
        deleteRes.FormattedSql.Should().Contain("DELETE");
    }

    [Fact]
    public void Format_WithCteQuery_Should_FormatCorrectly()
    {
        // Arrange
        var request = new SqlFormatterRequest
        {
            Sql = "with ActiveUsers as (select id from users where active=1) select * from ActiveUsers"
        };

        // Act
        var response = _service.Format(request);

        // Assert
        response.IsValid.Should().BeTrue();
        response.FormattedSql.Should().Contain("WITH");
        response.FormattedSql.Should().Contain("AS");
    }

    [Fact]
    public void Format_WithInvalidSqlSyntax_Should_ReturnErrorsAndRawSql()
    {
        // Arrange
        var request = new SqlFormatterRequest
        {
            Sql = "SELECT FROM WHERE" // Blatantly invalid SQL
        };

        // Act
        var response = _service.Format(request);

        // Assert
        response.IsValid.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.FormattedSql.Should().Be("SELECT FROM WHERE");
    }

    [Fact]
    public void Minify_WithValidSql_Should_CompressQuerySingleLine()
    {
        // Arrange
        var request = new SqlFormatterRequest
        {
            Sql = @"SELECT Id, Name, Email 
                    FROM Users 
                    WHERE IsActive = 1 
                    ORDER BY Name;"
        };

        // Act
        var response = _service.Minify(request);

        // Assert
        response.IsValid.Should().BeTrue();
        response.FormattedSql.Should().Be("SELECT Id,Name,Email FROM Users WHERE IsActive=1 ORDER BY Name;");
    }
}
