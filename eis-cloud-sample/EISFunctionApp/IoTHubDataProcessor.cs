using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace EISFunctionApp
{
    public static class IoTHubDataProcessor
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IoTHubDataProcessor")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IoTHubConnString", ConsumerGroup = "function")] EventData message, ILogger log)
        {
            var body = Encoding.UTF8.GetString(message.Body.Array);

            log.LogInformation($"C# IoT Hub trigger function processed a message: {body}");

            var defects = new List<DefectLocation>();

            try
            {
                var defectInfo = JsonConvert.DeserializeObject<DefectInfo>(body);
                if (defectInfo.defects == null || defectInfo.defects.Length <= 0)
                {
                    return;
                }

                foreach (var info in defectInfo.defects)
                {
                    var defect = new DefectLocation()
                    {
                        DefectType = info.type,
                        DetectedTime = DateTime.Parse(message.SystemProperties["iothub-enqueuedtime"].ToString()),
                        DeviceId = message.SystemProperties["iothub-connection-device-id"].ToString(),
                        ModuleId = message.SystemProperties["iothub-connection-module-id"].ToString(),
                        tl_x = info.tl[0],
                        tl_y = info.tl[1],
                        br_x = info.br[0],
                        br_y = info.br[1]
                    };
                    defects.Add(defect);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Data parsing error: {ex.ToString()}");
            }

            try
            {
                var connstr = "Server=tcp:gabrieleis02server.database.windows.net,1433;Initial Catalog=gabrieleis02sqldatabase;Persist Security Info=False;User ID=gabrieladmin;Password=openvino@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO [dbo].[DefectLocation] ([DetectedTime],[DeviceId],[ModuleId],[DefectType],[tl_x],[tl_y],[br_x],[br_y]) VALUES ");
                    sb.Append("(@detectedTime,@deviceId,@moduleId,@defectType,@tlx,@tly,@brx,@bry)");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.Add("@detectedTime", SqlDbType.DateTime2);
                        command.Parameters.Add("@deviceId", SqlDbType.NVarChar);
                        command.Parameters.Add("@moduleId", SqlDbType.NVarChar);
                        command.Parameters.Add("@defectType", SqlDbType.Int);
                        command.Parameters.Add("@tlx", SqlDbType.Int);
                        command.Parameters.Add("@tly", SqlDbType.Int);
                        command.Parameters.Add("@brx", SqlDbType.Int);
                        command.Parameters.Add("@bry", SqlDbType.Int);

                        command.CommandType = CommandType.Text;

                        defects.ForEach((x) =>
                        {
                            command.Parameters["@detectedTime"].Value = x.DetectedTime;
                            command.Parameters["@deviceId"].Value = x.DeviceId;
                            command.Parameters["@moduleId"].Value = x.ModuleId;
                            command.Parameters["@defectType"].Value = x.DefectType;
                            command.Parameters["@tlx"].Value = x.tl_x;
                            command.Parameters["@tly"].Value = x.tl_y;
                            command.Parameters["@brx"].Value = x.br_x;
                            command.Parameters["@bry"].Value = x.br_y;
                            if (command.ExecuteNonQuery() != 1)
                            {
                                throw new InvalidProgramException();
                            }

                            log.LogInformation(JsonConvert.SerializeObject(x));
                        });
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                log.LogError($"InsertInfo Error: {ex.ToString()}.");
            }
        }
    }
}