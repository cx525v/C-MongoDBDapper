using DataAccess.Interfaces;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class MongoVertexRepository : IMongoVertexRepository
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<Vertex> _vertexCollection;
        private string collectionName = "vertex";
        private string databaseName = System.Configuration.ConfigurationManager.AppSettings["mongoDatabaseName"].ToString();

        public MongoVertexRepository()
        {
            _client = new MongoClient();
            SetupDB();
        }
        public MongoVertexRepository(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                 _client = new MongoClient();
            else
                _client = new MongoClient(connectionString);

            SetupDB();
        }

        private void SetupDB()
        {
            _database = _client.GetDatabase(databaseName);
            DropCollection();
            CreateCollection();
            _vertexCollection = _database.GetCollection<Vertex>(collectionName);


        }


        public void CreateCollection()
        {
            _database.CreateCollection(collectionName);
        }
        public void DropCollection()
        {
            try
            {
                _database.DropCollection(collectionName);
            }
            catch
            {
                //todo
            }
        }


        //public async Task InsertVertex(MongoVertex vertex)
        //{            
        //    await _vertexCollection.InsertOneAsync(vertex);
        //}

        public void InsertVertex(Vertex vertex)
        {
            _vertexCollection.InsertOne(vertex);           
        }

        private void Insert(int X, int Y)
        {
            Vertex vertex = new Vertex(X,Y);        
            InsertVertex(vertex);
        }

        private void Insert(Vertex vertex)
        {
            Insert(vertex.X, vertex.Y);
        }

        private void Insert(List<Vertex> list)
        {         
           _vertexCollection.InsertMany(list);
            list = null;
        }

        public async Task DeleteAll()
        {
            var filter = new BsonDocument();
            var result = await _vertexCollection.DeleteManyAsync(filter);

        }

        public long GetCleanedVertices()
        {

            var option = new AggregateOptions { AllowDiskUse = true };
            var pipeline = new BsonDocument[] {
                new BsonDocument{                    
                         {

                          "$group", new BsonDocument{  {"_id", new BsonDocument { { "X", "$X" }, { "Y", "$Y" } } },

                         }

                      }
                    }
               };
          

            long count = 0;
            using (IAsyncCursor<BsonDocument> cursor = _vertexCollection.Aggregate<BsonDocument>(pipeline, option))
            {
                while ( cursor.MoveNext())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    count+= batch.Count();                
                }
            }

            return count;

        }

        private void CreateUser()
        {

            var writeConcern = WriteConcern.WMajority.With(wTimeout: TimeSpan.FromMilliseconds(5000));

            // Construct the createUser command.
            var command = new BsonDocument
                            {
                                { "createUser", "myUser" },
                                { "pwd", "changeMe" },
                                { "roles", new BsonArray
                                    {
                                         "readWrite"
                                    }},
                                { "writeConcern", writeConcern.ToBsonDocument() }
                            };


            _database.RunCommand<BsonDocument>(command);

        }

        private void SaveMove(Vertex startVertex, Move move)
        {
            try
            {

                switch (move.Direction)
                {
                    case Directions.E:
                        MoveEast(startVertex, move.Steps);
                        break;
                    case Directions.W:
                        MoveWest(startVertex, move.Steps);
                        break;

                    case Directions.S:
                        MoveSouth(startVertex, move.Steps);
                        break;

                    case Directions.N:
                        MoveNorth(startVertex, move.Steps);
                        break;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public long SaveCleanedVetex(CleanTask task)
        {
            DateTime d1 = DateTime.Now;

            Vertex startVertex = task.StartLocation;
            Insert(startVertex);
            foreach (Move move in task.Moves)
            {
                SaveMove(startVertex, move);
                switch (move.Direction)
                {
                    case Directions.E:
                        startVertex.X += move.Steps;
                        if (startVertex.X > Globales.MAX_X)
                            startVertex.X = Globales.MAX_X;
                        break;
                    case Directions.W:
                        startVertex.X -= move.Steps;
                        if (startVertex.X < Globales.NEGETIVE_MAX_X)
                            startVertex.X = Globales.NEGETIVE_MAX_X;
                        break;

                    case Directions.S:
                        startVertex.Y -= move.Steps;
                        if (startVertex.Y < Globales.NEGETIVE_MAX_Y)
                            startVertex.Y = Globales.NEGETIVE_MAX_Y;
                        break;

                    case Directions.N:
                        startVertex.Y += move.Steps;
                        if (startVertex.Y > Globales.MAX_Y)
                            startVertex.Y = Globales.MAX_Y;
                        break;
                }
            }

            DateTime d2 = DateTime.Now;
            long count = GetCleanedVertices();
            DateTime d3 = DateTime.Now;

            TimeSpan t1 = d2 - d1;
            TimeSpan t2 = d3 - d2;
            return count;
        }

        private void MoveEast(Vertex startVertex, int steps)
        {
            int X = startVertex.X;
            int Y = startVertex.Y;
            List<Vertex> list = new List<Vertex>();
            for (int i = X + 1; i <= X + steps && i <= Globales.MAX_X; i++)
            {
                list.Add(new Vertex(i, Y));
            }

            Insert(list);
        }

        private void MoveWest(Vertex startVertex, int steps)
        {
            int X = startVertex.X;
            int Y = startVertex.Y;
            List<Vertex> list = new List<Vertex>();
            for (int i = X - 1; i >= X - steps && i >= Globales.NEGETIVE_MAX_X; i--)
            {
                list.Add(new Vertex(i,Y));
               
            }

            Insert(list);
        }


        private void MoveNorth(Vertex startVertex, int steps)
        {
            int X = startVertex.X;
            int Y = startVertex.Y;
            List<Vertex> list = new List<Vertex>();
            for (int i = Y + 1; i <= Y + steps && i <= Globales.MAX_Y; i++)
            {
                list.Add(new Vertex(X, i));                     
            }

            Insert(list);
        }


        private void MoveSouth(Vertex startVertex, int steps)
        {
            int X = startVertex.X;
            int Y = startVertex.Y;
            List<Vertex> list = new List<Vertex>();
            for (int i = Y - 1; i >= Y - steps && i >= Globales.NEGETIVE_MAX_Y; i--)
            {
                list.Add(new Vertex(X, i));              
            }

            Insert(list);
        }


        public void Dispose()
        {
            DropCollection();
            _database = null;
        }
    }
}
