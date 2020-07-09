using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using MongoDB.Bson;
using MongoDB.Driver;

namespace tgBot
{
    class Program
    {
        private static MongoClient mongoClient = new MongoClient(mongoURI);
        private static IMongoDatabase database = mongoClient.GetDatabase("TelegramBot");
        public static IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(nameCollection);
        private static string log = "";
        const string BOT_TOKEN = ""; // токен бота
        static int numOfMess = 0;
        public const string nameCollection = "Log";
        public const string mongoURI = "mongodb+srv://Admin:mongo_admin@cluster0-56rhy.mongodb.net/TelegramBot?retryWrites=true&w=majority"; // база данных
        static async Task Main()
        {
            try
            {
                var client = new TelegramBotClient(BOT_TOKEN);
                client.OnMessage += BotOnMessageReceived;
                client.OnMessageEdited += BotOnMessageReceived;
                client.StartReceiving();

                try
                {
                    var filter = new BsonDocument();
                    using (var cursor = await collection.FindAsync(filter))
                    {
                        while (await cursor.MoveNextAsync())
                        {
                            var loger = cursor.Current;

                            foreach (BsonDocument lg in loger)
                            {
                                if (numOfMess == 0)
                                {
                                    log += lg.GetElement("text");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                while (true)
                {
                    Console.WriteLine("0 - exit\n1 - show log\n2 - clear log\n3 - numb of sms\nEnter num");
                    string id = Console.ReadLine();

                    if (id == "0") break;

                    else if (id == "1")
                    {
                        try
                        {
                            var filter = new BsonDocument();
                            using (var cursor = await collection.FindAsync(filter))
                            {
                                while (await cursor.MoveNextAsync())
                                {
                                    var loger = cursor.Current;

                                    foreach (BsonDocument lg in loger)
                                    {
                                        Console.WriteLine(lg.ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    else if (id == "2")
                    {
                        Saver("", collection);
                        log = "";
                        Console.WriteLine("Log clear");
                    }

                    else if (id == "3")
                    {
                        Console.WriteLine(numOfMess);
                    }

                    Console.ReadKey();
                    Console.Clear();
                }

                client.StopReceiving();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var chatId = messageEventArgs.Message.Chat.Id;
            var message = messageEventArgs.Message.Text;
            try
            {
                string str = $"Chat Id:{chatId}    User message:{message}    ";
                log += str;
                Saver(log, collection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            numOfMess++;
        }

        public static void Saver(string msg, IMongoCollection<BsonDocument> collection)
        {
            collection.UpdateOne(new BsonDocument("log", "log"), new BsonDocument("$set", new BsonDocument("text", msg)));
        }
    }
}