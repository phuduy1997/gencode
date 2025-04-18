using System;
using System.IO;
using System.Linq;
using System.Text;
using SqlGeneratorApp.Models;

namespace SqlGeneratorApp.Utils
{
    public enum FrameworkType
    {
        DotNetFramework,
        DotNetCore
    }

    public class CodeGenerator
    {
        private ModelInfo _model;
        private FrameworkType _frameworkType;

        public CodeGenerator(ModelInfo model, FrameworkType frameworkType = FrameworkType.DotNetFramework)
        {
            _model = model;
            _frameworkType = frameworkType;
        }

        public string GenerateModelClass()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.Runtime.Serialization;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Models");
            sb.AppendLine("{");
            sb.AppendLine("    [DataContract]");
            sb.AppendLine($"    public class {_model.ModelName}Model");
            sb.AppendLine("    {");
            
            foreach (var field in _model.Fields)
            {
                // Add attributes
                if (field.IsPrimaryKey)
                {
                    sb.AppendLine("        [Key]");
                }
                
                if (!field.IsNullable && field.GetCSharpType() == "string")
                {
                    sb.AppendLine("        [Required]");
                }
                
                if (field.MaxLength.HasValue && field.MaxLength > 0 && field.GetCSharpType() == "string")
                {
                    sb.AppendLine($"        [MaxLength({field.MaxLength.Value})]");
                }
                
                // DataMember attribute
                sb.AppendLine("        [DataMember]");
                
                // Property declaration
                sb.AppendLine($"        public {field.GetCSharpType()} {field.Name} {{ get; set; }}");
                sb.AppendLine();
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateRepositoryInterface()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine($"using {_model.ModelName}.Models;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Data");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{_model.ModelName}Repository");
            sb.AppendLine("    {");
            
            // Get by primary key
            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count > 0)
            {
                string pkParams = string.Join(", ", pkFields.Select(f => $"{f.GetCSharpType()} {f.Name.ToLower()}"));
                sb.AppendLine($"        Task<{_model.ModelName}> GetByIdAsync({pkParams});");
            }
            
            sb.AppendLine($"        Task<IEnumerable<{_model.ModelName}>> GetAllAsync();");
            sb.AppendLine($"        Task<(IEnumerable<{_model.ModelName}> Items, int TotalCount)> GetByPageAsync(int pageNumber, int pageSize);");
            sb.AppendLine($"        Task<{_model.ModelName}> InsertAsync({_model.ModelName} entity);");
            sb.AppendLine($"        Task<bool> UpdateAsync({_model.ModelName} entity);");
            
            if (pkFields.Count > 0)
            {
                string pkParams = string.Join(", ", pkFields.Select(f => $"{f.GetCSharpType()} {f.Name.ToLower()}"));
                sb.AppendLine($"        Task<bool> DeleteAsync({pkParams});");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateRepository()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Dapper;");
            sb.AppendLine($"using {_model.ModelName}.Models;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Data");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {_model.ModelName}Repository : I{_model.ModelName}Repository");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly string _connectionString;");
            sb.AppendLine();
            
            sb.AppendLine($"        public {_model.ModelName}Repository(string connectionString)");
            sb.AppendLine("        {");
            sb.AppendLine("            _connectionString = connectionString;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GetById method
            var pkFields = _model.Fields.Where(f => f.IsPrimaryKey).ToList();
            if (pkFields.Count > 0)
            {
                string pkParams = string.Join(", ", pkFields.Select(f => $"{f.GetCSharpType()} {f.Name.ToLower()}"));
                string pkParamNames = string.Join(", ", pkFields.Select(f => $"@{f.Name}"));
                
                sb.AppendLine($"        public async Task<{_model.ModelName}> GetByIdAsync({pkParams})");
                sb.AppendLine("        {");
                sb.AppendLine("            using (var connection = new SqlConnection(_connectionString))");
                sb.AppendLine("            {");
                sb.AppendLine("                await connection.OpenAsync();");
                
                // Create parameter object
                if (pkFields.Count == 1)
                {
                    sb.AppendLine($"                return await connection.QuerySingleOrDefaultAsync<{_model.ModelName}>(");
                    sb.AppendLine($"                    \"sp_{_model.ModelName}_GetById\",");
                    sb.AppendLine($"                    new {{ {pkFields[0].Name} = {pkFields[0].Name.ToLower()} }},");
                    sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
                }
                else
                {
                    sb.AppendLine($"                var parameters = new DynamicParameters();");
                    foreach (var field in pkFields)
                    {
                        sb.AppendLine($"                parameters.Add(\"@{field.Name}\", {field.Name.ToLower()});");
                    }
                    
                    sb.AppendLine();
                    sb.AppendLine($"                return await connection.QuerySingleOrDefaultAsync<{_model.ModelName}>(");
                    sb.AppendLine($"                    \"sp_{_model.ModelName}_GetById\",");
                    sb.AppendLine("                    parameters,");
                    sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
                }
                
                sb.AppendLine("            }");
                sb.AppendLine("        }");
                sb.AppendLine();
            }
            
            // GetAll method
            sb.AppendLine($"        public async Task<IEnumerable<{_model.ModelName}>> GetAllAsync()");
            sb.AppendLine("        {");
            sb.AppendLine("            using (var connection = new SqlConnection(_connectionString))");
            sb.AppendLine("            {");
            sb.AppendLine("                await connection.OpenAsync();");
            sb.AppendLine($"                return await connection.QueryAsync<{_model.ModelName}>(");
            sb.AppendLine($"                    \"sp_{_model.ModelName}_GetAll\",");
            sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GetByPage method
            sb.AppendLine($"        public async Task<(IEnumerable<{_model.ModelName}> Items, int TotalCount)> GetByPageAsync(int pageNumber, int pageSize)");
            sb.AppendLine("        {");
            sb.AppendLine("            using (var connection = new SqlConnection(_connectionString))");
            sb.AppendLine("            {");
            sb.AppendLine("                await connection.OpenAsync();");
            sb.AppendLine();
            sb.AppendLine("                var parameters = new DynamicParameters();");
            sb.AppendLine("                parameters.Add(\"@PageNumber\", pageNumber);");
            sb.AppendLine("                parameters.Add(\"@PageSize\", pageSize);");
            sb.AppendLine();
            sb.AppendLine("                using (var multi = await connection.QueryMultipleAsync(");
            sb.AppendLine($"                    \"sp_{_model.ModelName}_GetByPage\",");
            sb.AppendLine("                    parameters,");
            sb.AppendLine("                    commandType: CommandType.StoredProcedure))");
            sb.AppendLine("                {");
            sb.AppendLine($"                    var items = await multi.ReadAsync<{_model.ModelName}>();");
            sb.AppendLine("                    var totalCount = await multi.ReadFirstAsync<int>();");
            sb.AppendLine("                    return (items, totalCount);");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Insert method
            sb.AppendLine($"        public async Task<{_model.ModelName}> InsertAsync({_model.ModelName} entity)");
            sb.AppendLine("        {");
            sb.AppendLine("            using (var connection = new SqlConnection(_connectionString))");
            sb.AppendLine("            {");
            sb.AppendLine("                await connection.OpenAsync();");
            sb.AppendLine();
            sb.AppendLine("                var parameters = new DynamicParameters();");
            
            foreach (var field in _model.Fields.Where(f => !f.IsIdentity))
            {
                sb.AppendLine($"                parameters.Add(\"@{field.Name}\", entity.{field.Name});");
            }
            
            sb.AppendLine();
            
            var identityField = _model.Fields.FirstOrDefault(f => f.IsIdentity);
            if (identityField != null)
            {
                sb.AppendLine($"                entity.{identityField.Name} = await connection.ExecuteScalarAsync<{identityField.GetCSharpType()}>(");
                sb.AppendLine($"                    \"sp_{_model.ModelName}_Insert\",");
                sb.AppendLine("                    parameters,");
                sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
            }
            else
            {
                sb.AppendLine("                await connection.ExecuteAsync(");
                sb.AppendLine($"                    \"sp_{_model.ModelName}_Insert\",");
                sb.AppendLine("                    parameters,");
                sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
            }
            
            sb.AppendLine();
            sb.AppendLine("                return entity;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Update method
            sb.AppendLine($"        public async Task<bool> UpdateAsync({_model.ModelName} entity)");
            sb.AppendLine("        {");
            sb.AppendLine("            using (var connection = new SqlConnection(_connectionString))");
            sb.AppendLine("            {");
            sb.AppendLine("                await connection.OpenAsync();");
            sb.AppendLine();
            sb.AppendLine("                var parameters = new DynamicParameters();");
            
            foreach (var field in _model.Fields)
            {
                sb.AppendLine($"                parameters.Add(\"@{field.Name}\", entity.{field.Name});");
            }
            
            sb.AppendLine();
            sb.AppendLine("                var affectedRows = await connection.ExecuteAsync(");
            sb.AppendLine($"                    \"sp_{_model.ModelName}_Update\",");
            sb.AppendLine("                    parameters,");
            sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
            sb.AppendLine();
            sb.AppendLine("                return affectedRows > 0;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Delete method
            if (pkFields.Count > 0)
            {
                string pkParams = string.Join(", ", pkFields.Select(f => $"{f.GetCSharpType()} {f.Name.ToLower()}"));
                
                sb.AppendLine($"        public async Task<bool> DeleteAsync({pkParams})");
                sb.AppendLine("        {");
                sb.AppendLine("            using (var connection = new SqlConnection(_connectionString))");
                sb.AppendLine("            {");
                sb.AppendLine("                await connection.OpenAsync();");
                sb.AppendLine();
                sb.AppendLine("                var parameters = new DynamicParameters();");
                
                foreach (var field in pkFields)
                {
                    sb.AppendLine($"                parameters.Add(\"@{field.Name}\", {field.Name.ToLower()});");
                }
                
                sb.AppendLine();
                sb.AppendLine("                var affectedRows = await connection.ExecuteAsync(");
                sb.AppendLine($"                    \"sp_{_model.ModelName}_Delete\",");
                sb.AppendLine("                    parameters,");
                sb.AppendLine("                    commandType: CommandType.StoredProcedure);");
                sb.AppendLine();
                sb.AppendLine("                return affectedRows > 0;");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateController()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            return _frameworkType == FrameworkType.DotNetCore 
                ? GenerateCoreController() 
                : GenerateFrameworkController();
        }

        private string GenerateFrameworkController()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Net;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine($"using {_model.ModelName}.Models;");
            sb.AppendLine($"using {_model.ModelName}.Services;");
            sb.AppendLine($"using {_model.ModelName}.Common;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Controllers");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {_model.ModelName}Controller : ApiController");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly I{_model.ModelName}Service _{_model.ModelName.ToLower()}Service;");
            sb.AppendLine();
            
            sb.AppendLine($"        public {_model.ModelName}Controller(I{_model.ModelName}Service {_model.ModelName.ToLower()}Service)");
            sb.AppendLine("        {");
            sb.AppendLine($"            _{_model.ModelName.ToLower()}Service = {_model.ModelName.ToLower()}Service;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GET: GetAll
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine("        public HttpResponseMessage GetAll()");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.GetAll();");
            sb.AppendLine("                //");
            sb.AppendLine("                var paging = Convert.ToDouble(response.Total / Configurations.PageSize);");
            sb.AppendLine("                paging = (response.Total % Configurations.PageSize == 0 ? paging : paging + 1);");
            sb.AppendLine("                var total = Math.Round(paging, MidpointRounding.AwayFromZero);");
            sb.AppendLine();
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine("                    PageIndex = 1,");
            sb.AppendLine($"                    {_model.ModelName}s = response.Models,");
            sb.AppendLine("                    Total = total.AsInt32(),");
            sb.AppendLine("                    TotalCount = response.Total");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GET: GetByPage
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine("        public HttpResponseMessage GetByPage([FromUri] GetByPageRequest request)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (request == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return Request.CreateResponse(HttpStatusCode.BadRequest);");
            sb.AppendLine("                }");
            sb.AppendLine("                //");
            sb.AppendLine("                request.PageSize = Configurations.PageSize;");
            sb.AppendLine("                if (request.PageIndex <= 0)");
            sb.AppendLine("                    request.PageIndex = 1;");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.GetByPage(request);");
            sb.AppendLine("                //");
            sb.AppendLine("                var paging = Convert.ToDouble(response.Total / Configurations.PageSize);");
            sb.AppendLine("                paging = (response.Total % Configurations.PageSize == 0 ? paging : paging + 1);");
            sb.AppendLine("                var total = Math.Round(paging, MidpointRounding.AwayFromZero);");
            sb.AppendLine();
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine("                    request.PageIndex,");
            sb.AppendLine($"                    {_model.ModelName}s = response.Models,");
            sb.AppendLine("                    Total = total.AsInt32(),");
            sb.AppendLine("                    TotalCount = response.Total");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GET: GetById
            var pkField = _model.Fields.FirstOrDefault(f => f.IsPrimaryKey);
            string pkType = pkField != null ? pkField.GetCSharpType() : "System.Int32";
            
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine($"        public HttpResponseMessage GetById([FromUri] GetByIdRequest<{pkType}> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (request?.Id == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return Request.CreateResponse(HttpStatusCode.BadRequest);");
            sb.AppendLine("                }");
            sb.AppendLine("                //Get by identity");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.GetById(request);");
            sb.AppendLine("                ");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine($"                    {_model.ModelName} = response.Model");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // POST: Save/Insert/Update
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Inserts or updates the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine($"        public HttpResponseMessage Save(SaveRequest<{_model.ModelName}Model> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (request?.Model == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return Request.CreateResponse(HttpStatusCode.BadRequest);");
            sb.AppendLine("                }");
            sb.AppendLine("                ");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.Save(request);");
            sb.AppendLine("                //");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine("                    response.Id");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // DELETE
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deletes the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine($"        public HttpResponseMessage Delete(DeleteByIdRequest<{pkType}> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (request?.Id == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return Request.CreateResponse(HttpStatusCode.BadRequest);");
            sb.AppendLine("                }");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.Delete(request);");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Request.CreateResponse(HttpStatusCode.OK, new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        private string GenerateCoreController()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.AspNetCore.Http;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine($"using {_model.ModelName}.Models;");
            sb.AppendLine($"using {_model.ModelName}.Services;");
            sb.AppendLine($"using {_model.ModelName}.Common;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Controllers");
            sb.AppendLine("{");
            sb.AppendLine("    [Route(\"api/[controller]\")]");
            sb.AppendLine("    [ApiController]");
            sb.AppendLine($"    public class {_model.ModelName}Controller : ControllerBase");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly I{_model.ModelName}Service _{_model.ModelName.ToLower()}Service;");
            sb.AppendLine();
            
            sb.AppendLine($"        public {_model.ModelName}Controller(I{_model.ModelName}Service {_model.ModelName.ToLower()}Service)");
            sb.AppendLine("        {");
            sb.AppendLine($"            _{_model.ModelName.ToLower()}Service = {_model.ModelName.ToLower()}Service;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GET: GetAll
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine("        public IActionResult GetAll()");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.GetAll();");
            sb.AppendLine("                //");
            sb.AppendLine("                var paging = Convert.ToDouble(response.Total / Configurations.PageSize);");
            sb.AppendLine("                paging = (response.Total % Configurations.PageSize == 0 ? paging : paging + 1);");
            sb.AppendLine("                var total = Math.Round(paging, MidpointRounding.AwayFromZero);");
            sb.AppendLine();
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine("                    PageIndex = 1,");
            sb.AppendLine($"                    {_model.ModelName}s = response.Models,");
            sb.AppendLine("                    Total = Convert.ToInt32(total),");
            sb.AppendLine("                    TotalCount = response.Total");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GET: GetByPage
            sb.AppendLine("        [HttpGet(\"GetByPage\")]");
            sb.AppendLine("        public IActionResult GetByPage([FromQuery] GetByPageRequest request)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (request == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return BadRequest();");
            sb.AppendLine("                }");
            sb.AppendLine("                //");
            sb.AppendLine("                request.PageSize = Configurations.PageSize;");
            sb.AppendLine("                if (request.PageIndex <= 0)");
            sb.AppendLine("                    request.PageIndex = 1;");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.GetByPage(request);");
            sb.AppendLine("                //");
            sb.AppendLine("                var paging = Convert.ToDouble(response.Total / Configurations.PageSize);");
            sb.AppendLine("                paging = (response.Total % Configurations.PageSize == 0 ? paging : paging + 1);");
            sb.AppendLine("                var total = Math.Round(paging, MidpointRounding.AwayFromZero);");
            sb.AppendLine();
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine("                    request.PageIndex,");
            sb.AppendLine($"                    {_model.ModelName}s = response.Models,");
            sb.AppendLine("                    Total = Convert.ToInt32(total),");
            sb.AppendLine("                    TotalCount = response.Total");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GET: GetById
            var pkField = _model.Fields.FirstOrDefault(f => f.IsPrimaryKey);
            string pkType = pkField != null ? pkField.GetCSharpType() : "System.Int32";
            
            sb.AppendLine("        [HttpGet(\"{id}\")]");
            sb.AppendLine($"        public IActionResult GetById({pkType} id)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                var request = new GetByIdRequest<{pkType}> {{ Id = id }};");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.GetById(request);");
            sb.AppendLine("                ");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine($"                    {_model.ModelName} = response.Model");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // POST: Save/Insert/Update
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Inserts or updates the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine($"        public IActionResult Save([FromBody] SaveRequest<{_model.ModelName}Model> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (request?.Model == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return BadRequest();");
            sb.AppendLine("                }");
            sb.AppendLine("                ");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.Save(request);");
            sb.AppendLine("                //");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success,");
            sb.AppendLine("                    response.Id");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // DELETE
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deletes the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"id\">The identifier.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        [HttpDelete(\"{id}\")]");
            sb.AppendLine($"        public IActionResult Delete({pkType} id)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                var request = new DeleteByIdRequest<{pkType}> {{ Id = id }};");
            sb.AppendLine($"                var response = _{_model.ModelName.ToLower()}Service.Delete(request);");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    response.Message,");
            sb.AppendLine("                    response.Success");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine("                LogExeption.LogErrorToDatabase(exception);");
            sb.AppendLine("                var message = $\"Error: {exception.Message}\";");
            sb.AppendLine("                return Ok(new");
            sb.AppendLine("                {");
            sb.AppendLine("                    Message = message,");
            sb.AppendLine("                    Success = false");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateServiceInterface()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine($"using {_model.ModelName}.Models;");
            sb.AppendLine($"using {_model.ModelName}.Common;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Services");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{_model.ModelName}Service");
            sb.AppendLine("    {");
            
            // Determine primary key type
            var pkField = _model.Fields.FirstOrDefault(f => f.IsPrimaryKey);
            string pkType = pkField != null ? pkField.GetCSharpType() : "System.Int32";
            
            // Save method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Saves the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine($"        SaveResponse<{pkType}> Save(SaveRequest<{_model.ModelName}Model> request);");
            
            // Delete method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deletes the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine($"        DeleteByIdResponse Delete(DeleteByIdRequest<{pkType}> request);");
            
            // GetAll method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets all.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine($"        GetAllResponse<{_model.ModelName}Model> GetAll();");
            
            // GetById method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the by identifier.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine($"        GetByIdResponse<{_model.ModelName}Model> GetById(GetByIdRequest<{pkType}> request);");
            
            // GetByPage method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the by page.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine($"        GetAllResponse<{_model.ModelName}Model> GetByPage(GetByPageRequest request);");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateService()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine($"using {_model.ModelName}.Models;");
            sb.AppendLine($"using {_model.ModelName}.Data;");
            sb.AppendLine($"using {_model.ModelName}.Common;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {_model.ModelName}.Services");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {_model.ModelName}Service : I{_model.ModelName}Service");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IRepository _respository;");
            sb.AppendLine();
            
            sb.AppendLine($"        public {_model.ModelName}Service(IRepository respository)");
            sb.AppendLine("        {");
            sb.AppendLine("            _respository = respository;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Determine primary key type
            var pkField = _model.Fields.FirstOrDefault(f => f.IsPrimaryKey);
            string pkType = pkField != null ? pkField.GetCSharpType() : "System.Int32";
            
            // Save method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Saves the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        /// <exception cref=\"System.ArgumentNullException\">request</exception>");
            sb.AppendLine($"        public SaveResponse<{pkType}> Save(SaveRequest<{_model.ModelName}Model> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (request == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                throw new ArgumentNullException(nameof(request));");
            sb.AppendLine("            }");
            sb.AppendLine("            //");
            sb.AppendLine($"            var procedure = StoredProcedures.PRC_{_model.ModelName.ToUpper()}_SAVE;");
            sb.AppendLine("            var model = request.Model;");
            sb.AppendLine();
            sb.AppendLine("            var result = _respository.ExecuteQuery<" + _model.ModelName + "Model>(procedure, new");
            sb.AppendLine("            {");
            
            // Add each field as a parameter
            foreach (var field in _model.Fields)
            {
                sb.AppendLine($"                model.{field.Name},");
            }
            
            sb.AppendLine("            }, CommandType.StoredProcedure);");
            sb.AppendLine("            //");
            sb.AppendLine("            var obj = result.Data?.FirstOrDefault();");
            sb.AppendLine($"            var response = new SaveResponse<{pkType}>");
            sb.AppendLine("            {");
            sb.AppendLine("                Success = result.Success,");
            sb.AppendLine("                Message = result.Message,");
            if (pkField != null)
            {
                sb.AppendLine($"                Id = obj.{pkField.Name}");
            }
            else
            {
                sb.AppendLine("                Id = 0");
            }
            sb.AppendLine("            };");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Delete method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deletes the specified request.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        /// <exception cref=\"System.ArgumentNullException\">request</exception>");
            sb.AppendLine($"        public DeleteByIdResponse Delete(DeleteByIdRequest<{pkType}> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (request == null)");
            sb.AppendLine("                throw new ArgumentNullException(nameof(request));");
            sb.AppendLine($"            var procedure = StoredProcedures.PRC_{_model.ModelName.ToUpper()}_DELETE;");
            sb.AppendLine("            var result = _respository.ExecuteNonQuery(procedure, new");
            sb.AppendLine("            {");
            sb.AppendLine("                request.Id");
            sb.AppendLine("            }, CommandType.StoredProcedure);");
            sb.AppendLine("            var response = new DeleteByIdResponse");
            sb.AppendLine("            {");
            sb.AppendLine("                Success = result.Success,");
            sb.AppendLine("                Message = result.Message");
            sb.AppendLine("            };");
            sb.AppendLine("            return response;");
            sb.AppendLine();
            
            // GetAll method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets all.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine($"        public GetAllResponse<{_model.ModelName}Model> GetAll()");
            sb.AppendLine("        {");
            sb.AppendLine($"            var procedure = StoredProcedures.PRC_{_model.ModelName.ToUpper()}_GET_ALL;");
            sb.AppendLine($"            var result = _respository.ExecuteQuery<{_model.ModelName}Model>(procedure, null, CommandType.StoredProcedure);");
            sb.AppendLine($"            var {_model.ModelName.ToLower()} = result.Data?.FirstOrDefault();");
            sb.AppendLine($"            var response = new GetAllResponse<{_model.ModelName}Model>");
            sb.AppendLine("            {");
            sb.AppendLine("                Success = result.Success,");
            sb.AppendLine("                Message = result.Message,");
            sb.AppendLine("                Models = result.Data.ToList(),");
            sb.AppendLine($"                Total = {_model.ModelName.ToLower()}?.TotalRowCount ?? 0");
            sb.AppendLine("            };");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GetById method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the by identifier.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        /// <exception cref=\"System.ArgumentNullException\">request</exception>");
            sb.AppendLine($"        public GetByIdResponse<{_model.ModelName}Model> GetById(GetByIdRequest<{pkType}> request)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (request == null)");
            sb.AppendLine("                throw new ArgumentNullException(nameof(request));");
            sb.AppendLine($"            var procedure = StoredProcedures.PRC_{_model.ModelName.ToUpper()}_GET_BY_ID;");
            sb.AppendLine("            var result = _respository.ExecuteQuery<" + _model.ModelName + "Model>(procedure, new");
            sb.AppendLine("            {");
            sb.AppendLine("                request.Id");
            sb.AppendLine("            }, CommandType.StoredProcedure);");
            sb.AppendLine($"            var response = new GetByIdResponse<{_model.ModelName}Model>");
            sb.AppendLine("            {");
            sb.AppendLine("                Success = result.Success,");
            sb.AppendLine("                Message = result.Message,");
            sb.AppendLine("                Model = result.Data?.FirstOrDefault()");
            sb.AppendLine("            };");
            sb.AppendLine("            return response;");
            sb.AppendLine();
            
            // GetByPage method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the by page.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"request\">The request.</param>");
            sb.AppendLine("        /// <returns></returns>");
            sb.AppendLine("        /// <exception cref=\"System.ArgumentNullException\">request</exception>");
            sb.AppendLine($"        public GetAllResponse<{_model.ModelName}Model> GetByPage(GetByPageRequest request)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (request == null)");
            sb.AppendLine("                throw new ArgumentNullException(nameof(request));");
            sb.AppendLine($"            var procedure = StoredProcedures.PRC_{_model.ModelName.ToUpper()}_GET_BY_PAGE;");
            sb.AppendLine("            var result = _respository.ExecuteQuery<" + _model.ModelName + "Model>(procedure, new");
            sb.AppendLine("            {");
            sb.AppendLine("                request.SearchKey,");
            sb.AppendLine("                request.PageIndex,");
            sb.AppendLine("                request.PageSize");
            sb.AppendLine("            }, CommandType.StoredProcedure);");
            sb.AppendLine($"            var {_model.ModelName.ToLower()} = result.Data?.FirstOrDefault();");
            sb.AppendLine($"            var response = new GetAllResponse<{_model.ModelName}Model>");
            sb.AppendLine("            {");
            sb.AppendLine("                Success = result.Success,");
            sb.AppendLine("                Message = result.Message,");
            sb.AppendLine("                Models = result.Data.ToList(),");
            sb.AppendLine($"                Total = {_model.ModelName.ToLower()}?.TotalRowCount ?? 0");
            sb.AppendLine("            };");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateCommonClasses()
        {
            StringBuilder sb = new StringBuilder();
            
            // Response Models
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.Serialization;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_model.ModelName}.Common");
            sb.AppendLine("{");
            
            // Base Response
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class ResponseBase");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public bool Success { get; set; }");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public string Message { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // GetByIdResponse
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class GetByIdResponse<T> : ResponseBase");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public T Model { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // GetAllResponse
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class GetAllResponse<T> : ResponseBase");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public List<T> Models { get; set; }");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public int Total { get; set; }");
            sb.AppendLine();
            sb.AppendLine("        public GetAllResponse()");
            sb.AppendLine("        {");
            sb.AppendLine("            Models = new List<T>();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // SaveResponse
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class SaveResponse<T> : ResponseBase");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public T Id { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // DeleteByIdResponse
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class DeleteByIdResponse : ResponseBase");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Request Models
            // GetByPage
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class GetByPageRequest");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public int PageIndex { get; set; }");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public int PageSize { get; set; }");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public string SearchText { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // GetById
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class GetByIdRequest<T>");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public T Id { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // SaveRequest
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class SaveRequest<T>");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public T Model { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // DeleteByIdRequest
            sb.AppendLine("    [DataContract]");
            sb.AppendLine("    public class DeleteByIdRequest<T>");
            sb.AppendLine("    {");
            sb.AppendLine("        [DataMember]");
            sb.AppendLine("        public T Id { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Configuration
            sb.AppendLine("    public static class Configurations");
            sb.AppendLine("    {");
            sb.AppendLine("        public static int PageSize { get; } = 10;");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateExtensions()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_model.ModelName}.Common");
            sb.AppendLine("{");
            
            sb.AppendLine("    public static class Extensions");
            sb.AppendLine("    {");
            sb.AppendLine("        public static int AsInt32(this double value)");
            sb.AppendLine("        {");
            sb.AppendLine("            return Convert.ToInt32(value);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public static int AsInt32(this object value)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (value == null) return 0;");
            sb.AppendLine("            return Convert.ToInt32(value);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateLogExeption()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_model.ModelName}.Common");
            sb.AppendLine("{");
            
            sb.AppendLine("    public static class LogExeption");
            sb.AppendLine("    {");
            sb.AppendLine("        public static void LogErrorToDatabase(Exception ex)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Implement logging to database logic here");
            sb.AppendLine("            // This is a placeholder method");
            sb.AppendLine("            Console.WriteLine($\"Error: {ex.Message}\");");
            sb.AppendLine("            Console.WriteLine($\"StackTrace: {ex.StackTrace}\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public string GenerateStoredProcedures()
        {
            if (_model == null || _model.Fields.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"namespace {_model.ModelName}.Data");
            sb.AppendLine("{");
            sb.AppendLine("    public static class StoredProcedures");
            sb.AppendLine("    {");
            sb.AppendLine($"        public const string PRC_{_model.ModelName.ToUpper()}_SAVE = \"PRC_{_model.ModelName.ToUpper()}_SAVE\";");
            sb.AppendLine($"        public const string PRC_{_model.ModelName.ToUpper()}_DELETE = \"PRC_{_model.ModelName.ToUpper()}_DELETE\";");
            sb.AppendLine($"        public const string PRC_{_model.ModelName.ToUpper()}_GET_ALL = \"PRC_{_model.ModelName.ToUpper()}_GET_ALL\";");
            sb.AppendLine($"        public const string PRC_{_model.ModelName.ToUpper()}_GET_BY_ID = \"PRC_{_model.ModelName.ToUpper()}_GET_BY_ID\";");
            sb.AppendLine($"        public const string PRC_{_model.ModelName.ToUpper()}_GET_BY_PAGE = \"PRC_{_model.ModelName.ToUpper()}_GET_BY_PAGE\";");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public void SaveAllFiles(string outputDirectory)
        {
            if (_model == null || _model.Fields.Count == 0)
                return;

            string baseDir = Path.Combine(outputDirectory, _model.ModelName);
            Directory.CreateDirectory(baseDir);
            
            string modelDir = Path.Combine(baseDir, "Models");
            string dataDir = Path.Combine(baseDir, "Data");
            string serviceDir = Path.Combine(baseDir, "Services");
            string controllerDir = Path.Combine(baseDir, "Controllers");
            string sqlDir = Path.Combine(baseDir, "SQL");
            string commonDir = Path.Combine(baseDir, "Common");
            
            Directory.CreateDirectory(modelDir);
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(serviceDir);
            Directory.CreateDirectory(controllerDir);
            Directory.CreateDirectory(sqlDir);
            Directory.CreateDirectory(commonDir);
            
            // Generate and save model class
            string modelContent = GenerateModelClass();
            File.WriteAllText(Path.Combine(modelDir, $"{_model.ModelName}Model.cs"), modelContent);
            
            // Generate and save repository interface and implementation
            string repositoryInterfaceContent = GenerateRepositoryInterface();
            File.WriteAllText(Path.Combine(dataDir, $"I{_model.ModelName}Repository.cs"), repositoryInterfaceContent);
            
            string repositoryContent = GenerateRepository();
            File.WriteAllText(Path.Combine(dataDir, $"{_model.ModelName}Repository.cs"), repositoryContent);
            
            // Generate and save StoredProcedures class
            string storedProceduresContent = GenerateStoredProcedures();
            File.WriteAllText(Path.Combine(dataDir, "StoredProcedures.cs"), storedProceduresContent);
            
            // Generate and save service interface and implementation
            string serviceInterfaceContent = GenerateServiceInterface();
            File.WriteAllText(Path.Combine(serviceDir, $"I{_model.ModelName}Service.cs"), serviceInterfaceContent);
            
            string serviceContent = GenerateService();
            File.WriteAllText(Path.Combine(serviceDir, $"{_model.ModelName}Service.cs"), serviceContent);
            
            // Generate and save controller
            string controllerContent = GenerateController();
            File.WriteAllText(Path.Combine(controllerDir, $"{_model.ModelName}Controller.cs"), controllerContent);
            
            // Generate and save common classes
            string commonContent = GenerateCommonClasses();
            File.WriteAllText(Path.Combine(commonDir, "ResponseModels.cs"), commonContent);
            
            string extensionsContent = GenerateExtensions();
            File.WriteAllText(Path.Combine(commonDir, "Extensions.cs"), extensionsContent);
            
            string logExeptionContent = GenerateLogExeption();
            File.WriteAllText(Path.Combine(commonDir, "LogExeption.cs"), logExeptionContent);
            
            // Generate and save SQL scripts
            SqlGenerator sqlGenerator = new SqlGenerator(_model);
            
            string createTableSql = sqlGenerator.GenerateCreateTableSql();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_CreateTable.sql"), createTableSql);
            
            // Use Save procedure instead of separate Insert and Update
            string saveProcSql = sqlGenerator.GenerateSaveProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_Save_Procedure.sql"), saveProcSql);
            
            string deleteProcSql = sqlGenerator.GenerateDeleteProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_Delete_Procedure.sql"), deleteProcSql);
            
            string getAllProcSql = sqlGenerator.GenerateGetAllProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_GetAll_Procedure.sql"), getAllProcSql);
            
            string getByIdProcSql = sqlGenerator.GenerateGetByIdProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_GetById_Procedure.sql"), getByIdProcSql);
            
            string getByPageProcSql = sqlGenerator.GenerateGetByPageProcedure();
            File.WriteAllText(Path.Combine(sqlDir, $"{_model.ModelName}_GetByPage_Procedure.sql"), getByPageProcSql);
        }
    }
}