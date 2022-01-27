using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using E3.AuditTrailManager.Model.Data;
using E3.AuditTrailManager.Model.Enums;
using E3.Mediator.Models;
using E3.Mediator.Services;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using Unity;

namespace E3.AuditTrailManager.Model
{
    public class AuditTrailManager : IAuditTrailManager
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;

        public AuditTrailManager(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, MediatorService mediatorService)
        {
            this.databaseWriter = databaseWriter;
            this.databaseReader = databaseReader;
            SubscribeToMediatorServiceEvents(mediatorService);
        }

        internal void SubscribeToMediatorServiceEvents(MediatorService mediatorService)
        {
            mediatorService.Register(InMemoryMediatorMessageContainer.RecordAudit, obj => {
                object[] objArray = obj as object[];
                IUnityContainer unityContainer = (IUnityContainer)objArray[0];
                unityContainer.Resolve<IAuditTrailManager>().RecordEventAsync(Convert.ToString(objArray[1]), Convert.ToString(objArray[2]), (EventTypeEnum)Enum.Parse(typeof(EventTypeEnum), Convert.ToString(objArray[3])));
            });
        }

        public IList<AuditEvent> GetAuditTrail(DateTime start, DateTime end)
        {
            string query = $"select * from dbo.AuditTrail where TimeStamp between '{start:yyyy-MM-dd HH:mm:ss}' and '{end:yyyy-MM-dd HH:mm:ss}' order by TimeStamp desc";

            return (from DataRow row in databaseReader.ExecuteReadCommand(query, CommandType.Text).AsEnumerable()
                    select new AuditEvent
                    {
                        NameOfUser = row["ScientistName"].ToString(),
                        TimeStamp = DateTime.Parse(row["TimeStamp"].ToString()),
                        EventCategory = (EventTypeEnum)Enum.Parse(typeof(EventTypeEnum), row["Category"].ToString()),
                        Message = row["AuditMessage"].ToString()
                    }).ToList();
        }

        public IList<AuditEvent> GetAuditTrail(bool prevSet=false, bool nextSet = false, DateTime dateTimePoint = default)
        {
            string query = string.Empty;
            if (prevSet)
            {
                query = $"select top(100) * from dbo.AuditTrail where TimeStamp>='{dateTimePoint:yyyy-MM-dd HH:mm:ss}' order by TimeStamp asc";
            }
            else if (nextSet)
            {
                query = $"select top(100) * from dbo.AuditTrail where TimeStamp<='{dateTimePoint:yyyy-MM-dd HH:mm:ss}' order by TimeStamp desc";
            }
            else
            {
                query = $"select top(100) * from dbo.AuditTrail order by TimeStamp desc";
            }
            return (from DataRow row in databaseReader.ExecuteReadCommand(query, CommandType.Text).AsEnumerable()
                    select new AuditEvent
                    {
                        NameOfUser = row["ScientistName"].ToString(),
                        TimeStamp = DateTime.Parse(row["TimeStamp"].ToString()),
                        EventCategory = (EventTypeEnum)Enum.Parse(typeof(EventTypeEnum), row["Category"].ToString()),
                        Message = row["AuditMessage"].ToString()
                    }).ToList();
        }

        public void RecordEventAsync(string eventMessage, string nameOfUser, EventTypeEnum auditCategory)
        {
            AuditEvent auditEvent = new AuditEvent
            {
                NameOfUser = nameOfUser,
                EventCategory = auditCategory,
                Message = eventMessage,
            };

            Task.Factory.StartNew(new Action<object>(SaveEventToDatabase), auditEvent);
        }

        private void SaveEventToDatabase(object auditEventObject)
        {
            AuditEvent auditEvent = (AuditEvent)auditEventObject;
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@Category", auditEvent.EventCategory.ToString(), DbType.String),
                new DbParameterInfo("@AuditMessage", auditEvent.Message, DbType.String),
                new DbParameterInfo("@ScientistName", auditEvent.NameOfUser, DbType.String),
            };
            databaseWriter.ExecuteWriteCommand("LogAuditTrail", CommandType.StoredProcedure, parameters);
            UpdateAuditTrailView?.Invoke(this, new EventArgs());
        }

        public Task RecordEventSync(string eventMessage, string nameOfUser, EventTypeEnum auditCategory)
        {
            AuditEvent auditEvent = new AuditEvent
            {
                NameOfUser = nameOfUser,
                EventCategory = auditCategory,
                Message = eventMessage,
            };

            return Task.Factory.StartNew(new Action<object>(SaveEventToDatabase), auditEvent);
        }

        public event EventHandler UpdateAuditTrailView;
    }
}
