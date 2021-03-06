﻿using System;

namespace Estrella.Database
{
    /// <summary>
    ///     Represents a storage database.
    /// </summary>
    public class Database
    {
        /// <summary>
        ///     Constructs a Database instance with given details.
        /// </summary>
        /// <param name="sName">The name of the database.</param>
        /// <param name="minPoolSize">The minimum connection pool size for the database.</param>
        /// <param name="maxPoolSize"> The maximum connection pool size for the database.</param>
        public Database(string sName, uint minPoolSize, uint maxPoolSize)
        {
            if (sName == null || sName.Length == 0)
                throw new ArgumentException(sName);

            mName = sName;
            mMinPoolSize = minPoolSize;
            mMaxPoolSize = maxPoolSize;
        }

        private readonly string mName;
        private readonly uint mMinPoolSize;
        private readonly uint mMaxPoolSize;

        /// <summary>
        ///     The name of the database to connect to.
        /// </summary>
        internal string Name
        {
            get { return mName; }
        }

        /// <summary>
        ///     The minimum connection pool size for the database.
        /// </summary>
        internal uint minPoolSize
        {
            get { return mMinPoolSize; }
        }

        /// <summary>
        ///     The maximum connection pool size for the database.
        /// </summary>
        internal uint maxPoolSize
        {
            get { return mMaxPoolSize; }
        }
    }
}