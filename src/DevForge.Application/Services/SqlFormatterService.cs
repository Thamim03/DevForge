using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DevForge.Application.Services;

/// <summary>
/// Service implementation of ISqlFormatterService using Microsoft.SqlServer.TransactSql.ScriptDom.
/// </summary>
public class SqlFormatterService : ISqlFormatterService
{
    public SqlFormatterResponse Format(SqlFormatterRequest request)
    {
        var response = new SqlFormatterResponse();
        var sql = request?.Sql;

        if (string.IsNullOrWhiteSpace(sql))
        {
            response.FormattedSql = string.Empty;
            response.IsValid = true;
            return response;
        }

        try
        {
            var parser = new TSql160Parser(initialQuotedIdentifiers: true);
            using var reader = new StringReader(sql);
            var fragment = parser.Parse(reader, out var parseErrors);

            if (parseErrors != null && parseErrors.Count > 0)
            {
                response.IsValid = false;
                foreach (var err in parseErrors)
                {
                    response.Errors.Add($"Line {err.Line}, Column {err.Column}: {err.Message}");
                }
                response.FormattedSql = sql;
                return response;
            }

            var generator = new Sql160ScriptGenerator(new SqlScriptGeneratorOptions
            {
                KeywordCasing = KeywordCasing.Uppercase,
                MultilineSelectElementsList = true,
                MultilineWherePredicatesList = true,
                MultilineInsertTargetsList = true,
                MultilineInsertSourcesList = true,
                AlignClauseBodies = true,
                IndentationSize = 4
            });

            generator.GenerateScript(fragment, out var formattedSql);
            response.FormattedSql = formattedSql;
            response.IsValid = true;
        }
        catch (Exception ex)
        {
            response.IsValid = false;
            response.Errors.Add($"Unexpected formatting error: {ex.Message}");
            response.FormattedSql = sql;
        }

        return response;
    }

    public SqlFormatterResponse Minify(SqlFormatterRequest request)
    {
        var response = new SqlFormatterResponse();
        var sql = request?.Sql;

        if (string.IsNullOrWhiteSpace(sql))
        {
            response.FormattedSql = string.Empty;
            response.IsValid = true;
            return response;
        }

        try
        {
            var parser = new TSql160Parser(initialQuotedIdentifiers: true);
            using var reader = new StringReader(sql);
            var fragment = parser.Parse(reader, out var parseErrors);

            if (parseErrors != null && parseErrors.Count > 0)
            {
                response.IsValid = false;
                foreach (var err in parseErrors)
                {
                    response.Errors.Add($"Line {err.Line}, Column {err.Column}: {err.Message}");
                }
                response.FormattedSql = sql;
                return response;
            }

            // Minify by removing unnecessary spaces and newlines
            var minified = Regex.Replace(sql, @"\s+", " ").Trim();
            minified = Regex.Replace(minified, @"\s*([,=\(\)<>;])\s*", "$1");

            response.FormattedSql = minified;
            response.IsValid = true;
        }
        catch (Exception ex)
        {
            response.IsValid = false;
            response.Errors.Add($"Unexpected minification error: {ex.Message}");
            response.FormattedSql = sql;
        }

        return response;
    }
}
