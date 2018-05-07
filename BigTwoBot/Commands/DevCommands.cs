﻿using BigTwoBot.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Database;
using Telegram.Bot.Types.Enums;
using static BigTwoBot.Helpers;

namespace BigTwoBot
{
    public partial class Commands
    {
        [Command(Trigger = "update", DevOnly = true)]
        public static void Update(Message msg, string[] args)
        {
            if (msg.Date > DateTime.UtcNow.AddSeconds(-3))
            {
                Process.Start(Path.Combine(@"C:\BigTwoBot\", "Updater.exe"), msg.Chat.Id.ToString());
                Program.MaintMode = true;
                new Thread(CheckCurrentGames).Start();
            }
        }

        public static void CheckCurrentGames()
        {
            while (Bot.Games.Count > 0)
                Thread.Sleep(1000);
            Environment.Exit(1);
            return;
        }

        [Command(Trigger = "sql", DevOnly = true)]
        public static void Sql(Message msg, string[] args)
        {
            if (args.Length == 1)
            {
                msg.Reply("You must enter a sql query.");
                return;
            }
            using (var db = new BigTwoDb())
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                string raw = "";

                var queries = args[1].Split(';');
                foreach (var sql in queries)
                {
                    using (var comm = conn.CreateCommand())
                    {
                        comm.CommandText = sql;
                        if (string.IsNullOrEmpty(sql)) continue;
                        var reader = comm.ExecuteReader();
                        var result = "";
                        if (reader.HasRows)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                                raw += $"<code>{reader.GetName(i).FormatHTML()}</code>" + (i == reader.FieldCount - 1 ? "" : " - ");
                            result += raw + Environment.NewLine;
                            raw = "";
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                    raw += (reader.IsDBNull(i) ? "<i>NULL</i>" : $"<code>{reader[i].ToString().FormatHTML()}</code>") + (i == reader.FieldCount - 1 ? "" : " - ");
                                result += raw + Environment.NewLine;
                                raw = "";
                            }
                        }

                        result += reader.RecordsAffected == -1 ? "" : (reader.RecordsAffected + " records affected");
                        result = !String.IsNullOrEmpty(result) ? result : (sql.ToLower().StartsWith("select") ? "Nothing found" : "Done.");
                        msg.Reply(result);
                    }
                }
            }
        }

        [Attributes.Command(Trigger = "reloadlangs", DevOnly = true)]
        public static void ReloadLang(Message msg, string[] args)
        {
            Program.English = Helpers.ReadEnglish();
            Program.Langs = Helpers.ReadLanguageFiles();
            msg.Reply("Done.");
        }

        [Command(Trigger = "uploadlang", DevOnly = true)]
        public static void UploadLang(Message msg, string[] args)
        {
            try
            {
                var id = msg.Chat.Id;
                if (msg.ReplyToMessage?.Type != MessageType.DocumentMessage)
                {
                    Bot.Send(id, "Please reply to the file with /uploadlang");
                    return;
                }
                var fileid = msg.ReplyToMessage.Document?.FileId;
                if (fileid != null)
                    UploadFile(fileid, id,
                        msg.ReplyToMessage.Document.FileName,
                        msg.MessageId);
            }
            catch (Exception e)
            {
                Bot.Send(msg.Chat.Id, e.Message, parseMode: ParseMode.Default);
            }
        }
    }
}
