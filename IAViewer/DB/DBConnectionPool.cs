using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace IAViewer.DB
{
    public class DBConnectionPool
    {
        private ConcurrentBag<IDatabase> _databaseCollection;
        private Func<IDatabase> _databaseGenerator;

        public DBConnectionPool(Func<IDatabase> databaseGenerator)
        {
            if (databaseGenerator == null) throw new ArgumentNullException("databaseGenerator");
            _databaseCollection = new ConcurrentBag<IDatabase>();
            _databaseGenerator = databaseGenerator;
        }

        public IDatabase GetObject()
        {
            IDatabase item;
            if (_databaseCollection.TryTake(out item)) return item;
            return _databaseGenerator();
        }

        public void PutObject(IDatabase item)
        {
            _databaseCollection.Add(item);
        }

        public void CloseAllConnections()
        {
            if (_databaseCollection != null)
            {
                foreach (IDatabase database in _databaseCollection)
                {
                    database.CloseConnection();
                }
            }
        }
    }
}
