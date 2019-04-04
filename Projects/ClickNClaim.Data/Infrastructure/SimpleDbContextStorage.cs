﻿using System.Collections.Generic;
using System.Data.Entity;

namespace Infrastructure.Data
{
    public class SimpleDbContextStorage : IDbContextStorage
    {
        private Dictionary<string, DbContext> _storage = new Dictionary<string, DbContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDbContextStorage"/> class.
        /// </summary>
        public SimpleDbContextStorage() { }

        /// <summary>
        /// Returns the db context associated with the specified key or
		/// null if the specified key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public DbContext GetDbContextForKey(string key)
        {
            DbContext context;
            if (!this._storage.TryGetValue(key, out context))
                return null;
            return context;
        }


        /// <summary>
        /// Stores the db context into a dictionary using the specified key.
        /// If an object context already exists by the specified key, 
        /// it gets overwritten by the new object context passed in.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objectContext">The object context.</param>
        public void SetDbContextForKey(string key, DbContext context)
        {
            this._storage.Add(key, context);           
        }

        /// <summary>
        /// Returns all the values of the internal dictionary of db contexts.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DbContext> GetAllDbContexts()
        {
            return this._storage.Values;
        }
    }
}
