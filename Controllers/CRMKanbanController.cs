using System.Security.AccessControl;
using System.Dynamic;
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CrmKanbanController : ControllerBase
    {


        [HttpGet("[action]")]
        public IActionResult columnslist([FromQuery] string userlogin, [FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;

            _cmd = "exec dbo.[CRM_KANBAN_GET_Columns]  @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            var columns = new List<object>();
            var tasks = new Dictionary<string, List<object>>();

            foreach (DataRow row in dt.Rows)
            {
                var columnId = row["ColumnId"].ToString();
                var columnName = row["ColumnName"].ToString();

                if (!columns.Any(c => c.ToString() == columnId))
                {
                    columns.Add(new { id = columnId, name = columnName });
                }

                if (!tasks.ContainsKey(columnId))
                {
                    tasks[columnId] = new List<object>();
                }

                var taskId = row["TaskId"].ToString();
                var existingTask = tasks[columnId].FirstOrDefault(t => ((dynamic)t).id == taskId);

                if (existingTask == null)
                {
                    var task = new
                    {
                        id = taskId,
                        name = row["TaskName"].ToString(),
                        status = row["Status"].ToString(),


                        priority = row["Priority"].ToString(),

                        labels = new List<string>(),
                        description = row["TaskDescription"].ToString(),
                        attachments = new List<string>(), // Assuming attachments are handled separately
                        comments = new List<object>(),
                        assignee = new List<object>(),
                        due = new object[] { row["DueStart"], row["DueEnd"] },
                        reporter = new
                        {
                            id = row["ReporterId"].ToString(),
                            name = row["ReporterName"].ToString(),
                            avatarUrl = row["ReporterAvatarUrl"].ToString()
                        }
                    };
                    tasks[columnId].Add(task);
                    existingTask = task;
                }

                if (!string.IsNullOrEmpty(row["CommentId"].ToString()))
                {
                    ((dynamic)existingTask).comments.Add(new
                    {
                        id = row["CommentId"].ToString(),
                        name = row["Author"].ToString(),
                        message = row["CommentContent"].ToString(),
                        avatarUrl = row["CommentAvatar"].ToString(),
                        messageType = row["CommentMessageType"].ToString(),
                        createdAt = row["CommentCreatedAt"]
                    });
                }

                if (!string.IsNullOrEmpty(row["AssigneeId"].ToString()))
                {
                    ((dynamic)existingTask).assignee.Add(new
                    {
                        id = row["AssigneeId"].ToString(),
                        name = row["AssigneeName"].ToString(),
                        role = row["AssigneeRole"].ToString(),
                        email = row["AssigneeEmail"].ToString(),
                        status = row["AssigneeStatus"].ToString(),
                        address = row["AssigneeAddress"].ToString(),
                        avatarUrl = row["AssigneeAvatarUrl"].ToString(),
                        phoneNumber = row["AssigneePhoneNumber"].ToString(),
                        lastActivity = row["AssigneeLastActivity"]
                    });
                }
            }

            var response = new
            {
                board = new
                {
                    tasks,
                    columns
                }
            };

            return Ok(response);
        }


    }


}




















