using System;
using System.Linq;
using System.Text;
using SqlGeneratorApp.Models;
using System.IO;

namespace SqlGeneratorApp.Utils
{
    public class SqlGenerator
    {
        private ModelInfo _model;

        public SqlGenerator(ModelInfo model)
        {
            _model = model;
        }

        public string GenerateCreateTableSql()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [dbo].[{_model.ModelName}] (");

            for (int i = 0; i < _model.Fields.Count; i++)
            {
                var field = _model.Fields[i];
                string line = $"    [{field.Name}] {field.GetSqlType()}";

                if (field.IsIdentity)
                    line += " IDENTITY(1,1)";

                if (!field.IsNullable)
                    line += " NOT NULL";
                else
                    line += " NULL";

                if (!string.IsNullOrEmpty(field.DefaultValue))
                    line += $" DEFAULT {field.DefaultValue}";

                if (i < _model.Fields.Count - 1)
                    line += ",";

                sb.AppendLine(line);
            }

            // Add primary key constraint if any field is marked as PK
            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count > 0)
            {
                sb.AppendLine($"    CONSTRAINT [PK_{_model.ModelName}] PRIMARY KEY CLUSTERED (");
                for (int i = 0; i < pkFields.Count; i++)
                {
                    sb.Append($"        [{pkFields[i].Name}] ASC");
                    if (i < pkFields.Count - 1)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }
                sb.AppendLine("    )");
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        public string GenerateInsertProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            string procName = $"sp_{_model.ModelName}_Insert";

            sb.AppendLine($"CREATE PROCEDURE [dbo].[{procName}]");

            // Parameters
            var nonIdentityFields = _model.Fields.Where(f => !f.IsIdentity).ToList();
            for (int i = 0; i < nonIdentityFields.Count; i++)
            {
                var field = nonIdentityFields[i];
                sb.Append($"    @{field.Name} {field.GetSqlType()}");
                
                if (i < nonIdentityFields.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            sb.Append($"    INSERT INTO [dbo].[{_model.ModelName}] (");
            
            // Column names
            for (int i = 0; i < nonIdentityFields.Count; i++)
            {
                sb.Append($"[{nonIdentityFields[i].Name}]");
                if (i < nonIdentityFields.Count - 1)
                    sb.Append(", ");
            }
            
            sb.AppendLine(")");
            sb.Append("    VALUES (");
            
            // Parameter names
            for (int i = 0; i < nonIdentityFields.Count; i++)
            {
                sb.Append($"@{nonIdentityFields[i].Name}");
                if (i < nonIdentityFields.Count - 1)
                    sb.Append(", ");
            }
            
            sb.AppendLine(")");
            
            // If there's an identity column, return its value
            var identityField = _model.Fields.FirstOrDefault(f => f.IsIdentity);
            if (identityField != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    SELECT SCOPE_IDENTITY() AS [{identityField.Name}]");
            }
            
            sb.AppendLine("END");
            return sb.ToString();
        }

        public string GenerateUpdateProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count == 0)
                return "-- Cannot generate Update procedure without primary key fields.";

            StringBuilder sb = new StringBuilder();
            string procName = $"sp_{_model.ModelName}_Update";

            sb.AppendLine($"CREATE PROCEDURE [dbo].[{procName}]");

            // Parameters (all fields)
            for (int i = 0; i < _model.Fields.Count; i++)
            {
                var field = _model.Fields[i];
                sb.Append($"    @{field.Name} {field.GetSqlType()}");
                
                if (i < _model.Fields.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            sb.AppendLine($"    UPDATE [dbo].[{_model.ModelName}]");
            sb.AppendLine("    SET");
            
            // Non-PK fields for updating
            var nonPkFields = _model.Fields.Where(f => !f.IsPrimaryKey).ToList();
            for (int i = 0; i < nonPkFields.Count; i++)
            {
                sb.Append($"        [{nonPkFields[i].Name}] = @{nonPkFields[i].Name}");
                if (i < nonPkFields.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("    WHERE");
            
            // PK fields for condition
            for (int i = 0; i < pkFields.Count; i++)
            {
                sb.Append($"        [{pkFields[i].Name}] = @{pkFields[i].Name}");
                if (i < pkFields.Count - 1)
                    sb.AppendLine(" AND");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("END");
            return sb.ToString();
        }

        public string GenerateDeleteProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count == 0)
                return "-- Cannot generate Delete procedure without primary key fields.";

            StringBuilder sb = new StringBuilder();
            string procName = $"sp_{_model.ModelName}_Delete";

            sb.AppendLine($"CREATE PROCEDURE [dbo].[{procName}]");

            // Parameters (only PK fields)
            for (int i = 0; i < pkFields.Count; i++)
            {
                var field = pkFields[i];
                sb.Append($"    @{field.Name} {field.GetSqlType()}");
                
                if (i < pkFields.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            sb.AppendLine($"    DELETE FROM [dbo].[{_model.ModelName}]");
            sb.AppendLine("    WHERE");
            
            // PK fields for condition
            for (int i = 0; i < pkFields.Count; i++)
            {
                sb.Append($"        [{pkFields[i].Name}] = @{pkFields[i].Name}");
                if (i < pkFields.Count - 1)
                    sb.AppendLine(" AND");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("END");
            return sb.ToString();
        }

        public string GenerateGetAllProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            string procName = $"PRC_{_model.ModelName.ToUpper()}_GET_ALL";

            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            sb.Append($"    SELECT ");
            
            // All columns
            for (int i = 0; i < _model.Fields.Count; i++)
            {
                sb.Append($"[{_model.Fields[i].Name}]");
                if (i < _model.Fields.Count - 1)
                    sb.Append(", ");
            }
            
            // Add TotalRowCount column for consistent formatting
            sb.Append(", COUNT(*) OVER() AS TotalRowCount");
            
            sb.AppendLine();
            sb.AppendLine($"    FROM [dbo].[{_model.ModelName}]");
            sb.AppendLine("END");
            return sb.ToString();
        }

        public string GenerateGetByPageProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count == 0)
                pkFields.Add(_model.Fields.First()); // Use first field if no PK

            StringBuilder sb = new StringBuilder();
            string procName = $"PRC_{_model.ModelName.ToUpper()}_GET_BY_PAGE";

            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");
            sb.AppendLine("    @PageIndex INT = 1,");
            sb.AppendLine("    @PageSize INT = 10,");
            sb.AppendLine("    @SearchKey NVARCHAR(50) = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            sb.AppendLine("    -- Calculate the number of rows to skip");
            sb.AppendLine("    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;");
            sb.AppendLine();
            
            sb.Append($"    SELECT ");
            
            // All columns
            for (int i = 0; i < _model.Fields.Count; i++)
            {
                sb.Append($"[{_model.Fields[i].Name}]");
                if (i < _model.Fields.Count - 1)
                    sb.Append(", ");
            }
            
            // Add TotalRowCount column for consistent formatting with COUNT OVER()
            sb.Append(", COUNT(*) OVER() AS TotalRowCount");
            
            sb.AppendLine();
            sb.AppendLine($"    FROM [dbo].[{_model.ModelName}]");
            
            // Add search condition if there's a text field that could be searched
            var textFields = _model.Fields.Where(f => 
                f.GetCSharpType() == "string" && 
                !f.Name.Contains("Id", StringComparison.OrdinalIgnoreCase) &&
                !f.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            if (textFields.Any())
            {
                sb.AppendLine("    WHERE (@SearchKey IS NULL");
                sb.Append("        OR ");
                
                for (int i = 0; i < textFields.Count; i++)
                {
                    sb.Append($"[{textFields[i].Name}] LIKE '%' + @SearchKey + '%'");
                    if (i < textFields.Count - 1)
                        sb.AppendLine(" OR ");
                }
                
                sb.AppendLine(")");
            }
            
            sb.Append("    ORDER BY ");
            
            // Order by PK fields
            for (int i = 0; i < pkFields.Count; i++)
            {
                sb.Append($"[{pkFields[i].Name}]");
                if (i < pkFields.Count - 1)
                    sb.Append(", ");
            }
            
            sb.AppendLine();
            sb.AppendLine("    OFFSET @Offset ROWS");
            sb.AppendLine("    FETCH NEXT @PageSize ROWS ONLY;");
            sb.AppendLine("END");
            return sb.ToString();
        }

        public string GenerateSaveProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            
            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[PRC_{_model.ModelName.ToUpper()}_SAVE]");
            
            // Add parameters
            for (int i = 0; i < _model.Fields.Count; i++)
            {
                var field = _model.Fields[i];
                string sqlType = field.GetSqlType();
                
                if (field.IsIdentity && field.IsPrimaryKey)
                {
                    sb.AppendLine($"    @{field.Name} {sqlType} = NULL{(i < _model.Fields.Count - 1 ? "," : "")}");
                }
                else
                {
                    sb.AppendLine($"    @{field.Name} {sqlType}{(i < _model.Fields.Count - 1 ? "," : "")}");
                }
            }
            
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            // Add logic for INSERT or UPDATE
            if (pkFields.Count > 0)
            {
                var pkField = pkFields[0]; // Handle first PK for simplicity
                
                // For numeric identity PK, check if it's NULL or 0
                if (pkField.IsIdentity)
                {
                    if (pkField.GetCSharpType() == "int" || pkField.GetCSharpType() == "long")
                    {
                        sb.AppendLine($"    IF @{pkField.Name} IS NULL OR @{pkField.Name} = 0");
                    }
                    else if (pkField.GetCSharpType() == "Guid")
                    {
                        sb.AppendLine($"    IF @{pkField.Name} IS NULL OR @{pkField.Name} = '00000000-0000-0000-0000-000000000000'");
                    }
                    else
                    {
                        sb.AppendLine($"    IF @{pkField.Name} IS NULL");
                    }
                }
                else
                {
                    // If not identity, check if exists
                    sb.AppendLine($"    IF NOT EXISTS (SELECT 1 FROM [dbo].[{_model.ModelName}] WHERE {pkField.Name} = @{pkField.Name})");
                }
                
                sb.AppendLine("    BEGIN");
                sb.AppendLine("        -- Insert new record");
                sb.AppendLine($"        INSERT INTO [dbo].[{_model.ModelName}]");
                sb.AppendLine("        (");
                
                // Non-identity fields for insert
                var nonIdentityFields = _model.Fields.Where(f => !f.IsIdentity).ToList();
                for (int i = 0; i < nonIdentityFields.Count; i++)
                {
                    var field = nonIdentityFields[i];
                    sb.AppendLine($"            [{field.Name}]{(i < nonIdentityFields.Count - 1 ? "," : "")}");
                }
                
                sb.AppendLine("        )");
                sb.AppendLine("        VALUES");
                sb.AppendLine("        (");
                
                // Parameter values for non-identity fields
                for (int i = 0; i < nonIdentityFields.Count; i++)
                {
                    var field = nonIdentityFields[i];
                    sb.AppendLine($"            @{field.Name}{(i < nonIdentityFields.Count - 1 ? "," : "")}");
                }
                
                sb.AppendLine("        );");
                
                // For identity field, return the newly generated ID
                var identityField = _model.Fields.FirstOrDefault(f => f.IsIdentity);
                if (identityField != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("        -- Return the identity value");
                    sb.AppendLine("        SELECT SCOPE_IDENTITY() AS Id, 'Record inserted successfully' AS Message, 1 AS Success, 1 AS TotalRowCount;");
                }
                else if (pkField != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("        -- Return the primary key");
                    sb.AppendLine($"        SELECT @{pkField.Name} AS Id, 'Record inserted successfully' AS Message, 1 AS Success, 1 AS TotalRowCount;");
                }
                
                sb.AppendLine("    END");
                sb.AppendLine("    ELSE");
                sb.AppendLine("    BEGIN");
                sb.AppendLine("        -- Update existing record");
                sb.AppendLine($"        UPDATE [dbo].[{_model.ModelName}]");
                sb.AppendLine("        SET");
                
                // Get updateable fields (non-primary key)
                var updateableFields = _model.Fields.Where(f => !f.IsPrimaryKey).ToList();
                for (int i = 0; i < updateableFields.Count; i++)
                {
                    var field = updateableFields[i];
                    sb.AppendLine($"            [{field.Name}] = @{field.Name}{(i < updateableFields.Count - 1 ? "," : "")}");
                }
                
                sb.AppendLine("        WHERE");
                
                // Add WHERE condition for primary keys
                for (int i = 0; i < pkFields.Count; i++)
                {
                    var field = pkFields[i];
                    if (i > 0) sb.Append("          AND ");
                    else sb.Append("            ");
                    
                    sb.AppendLine($"[{field.Name}] = @{field.Name}{(i < pkFields.Count - 1 ? "" : ";")}");
                }
                
                sb.AppendLine();
                sb.AppendLine("        -- Return the primary key");
                sb.AppendLine($"        SELECT @{pkField.Name} AS Id, 'Record updated successfully' AS Message, 1 AS Success, 1 AS TotalRowCount;");
                sb.AppendLine("    END");
            }
            else
            {
                // If no primary key defined, always insert new record
                sb.AppendLine("    -- Insert new record");
                sb.AppendLine($"    INSERT INTO [dbo].[{_model.ModelName}]");
                sb.AppendLine("    (");
                
                // All fields except identity
                var insertFields = _model.Fields.Where(f => !f.IsIdentity).ToList();
                for (int i = 0; i < insertFields.Count; i++)
                {
                    var field = insertFields[i];
                    sb.AppendLine($"        [{field.Name}]{(i < insertFields.Count - 1 ? "," : "")}");
                }
                
                sb.AppendLine("    )");
                sb.AppendLine("    VALUES");
                sb.AppendLine("    (");
                
                // Values for all fields except identity
                for (int i = 0; i < insertFields.Count; i++)
                {
                    var field = insertFields[i];
                    sb.AppendLine($"        @{field.Name}{(i < insertFields.Count - 1 ? "," : "")}");
                }
                
                sb.AppendLine("    );");
                
                // For identity field, return the newly generated ID
                var identityField = _model.Fields.FirstOrDefault(f => f.IsIdentity);
                if (identityField != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("    -- Return the identity value");
                    sb.AppendLine("    SELECT SCOPE_IDENTITY() AS Id, 'Record saved successfully' AS Message, 1 AS Success, 1 AS TotalRowCount;");
                }
            }
            
            sb.AppendLine("END");
            
            return sb.ToString();
        }

        public string GenerateGetByIdProcedure()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count == 0)
                return "-- Cannot generate GetById procedure without primary key fields.";

            StringBuilder sb = new StringBuilder();
            string procName = $"PRC_{_model.ModelName.ToUpper()}_GET_BY_ID";

            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");

            // Parameters (only PK fields)
            for (int i = 0; i < pkFields.Count; i++)
            {
                var field = pkFields[i];
                sb.Append($"    @{field.Name} {field.GetSqlType()}");
                
                if (i < pkFields.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            
            sb.Append($"    SELECT ");
            
            // All columns
            for (int i = 0; i < _model.Fields.Count; i++)
            {
                sb.Append($"[{_model.Fields[i].Name}]");
                if (i < _model.Fields.Count - 1)
                    sb.Append(", ");
            }
            
            // Add TotalRowCount for response format consistency
            sb.Append(", 1 AS TotalRowCount");
            
            sb.AppendLine();
            sb.AppendLine($"    FROM [dbo].[{_model.ModelName}]");
            sb.AppendLine("    WHERE");
            
            // PK fields for condition
            for (int i = 0; i < pkFields.Count; i++)
            {
                sb.Append($"        [{pkFields[i].Name}] = @{pkFields[i].Name}");
                if (i < pkFields.Count - 1)
                    sb.AppendLine(" AND");
                else
                    sb.AppendLine();
            }
            
            sb.AppendLine("END");
            return sb.ToString();
        }

        public void SaveAllFiles(string outputDirectory)
        {
            if (_model == null || _model.Fields.Count == 0)
                return;

            string sqlDir = outputDirectory;
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(sqlDir))
            {
                Directory.CreateDirectory(sqlDir);
            }
            
            // Save CREATE TABLE script
            string createTableSql = GenerateCreateTableSql();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_CreateTable.sql"), createTableSql);
            
            // Save SAVE procedure (replaces INSERT and UPDATE)
            string saveProcSql = GenerateSaveProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_Save_Procedure.sql"), saveProcSql);
            
            // Save DELETE procedure
            string deleteProcSql = GenerateDeleteProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_Delete_Procedure.sql"), deleteProcSql);
            
            // Save GET procedures
            string getAllProcSql = GenerateGetAllProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_GetAll_Procedure.sql"), getAllProcSql);
            
            string getByIdProcSql = GenerateGetByIdProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_GetById_Procedure.sql"), getByIdProcSql);
            
            string getByPageProcSql = GenerateGetByPageProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_GetByPage_Procedure.sql"), getByPageProcSql);
        }
    }
} 