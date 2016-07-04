﻿//-----------------------------------------------------------------------
// <copyright file="MySqlSnapshotQueryExecutor.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using Akka.Persistence.Sql.Common.Snapshot;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Snapshot
{
    public class MySqlSnapshotQueryExecutor : AbstractQueryExecutor
    {
        public MySqlSnapshotQueryExecutor(QueryConfiguration configuration, Akka.Serialization.Serialization serialization) : base(configuration, serialization)
        {
            CreateSnapshotTableSql = $@"
                CREATE TABLE IF NOT EXISTS {configuration.FullSnapshotTableName} (
                    {configuration.PersistenceIdColumnName} VARCHAR(255) NOT NULL,
                    {configuration.SequenceNrColumnName} INTEGER(8) NOT NULL,
                    {configuration.TimestampColumnName} INTEGER(8) NOT NULL,
                    {configuration.ManifestColumnName} VARCHAR(255) NOT NULL,
                    {configuration.PayloadColumnName} BLOB NOT NULL,
                    PRIMARY KEY ({configuration.PersistenceIdColumnName}, {configuration.SequenceNrColumnName})
                );";
        }

        protected override string CreateSnapshotTableSql { get; }

        protected override DbCommand CreateCommand(DbConnection connection) => new MySqlCommand { Connection = (MySqlConnection)connection };

        protected override void SetTimestampParameter(DateTime timestamp, DbCommand command) => AddParameter(command, "@Timestamp", DbType.Int64, timestamp.Ticks);

        protected override SelectedSnapshot ReadSnapshot(DbDataReader reader)
        {
            var persistenceId = reader.GetString(0);
            var sequenceNr = reader.GetInt64(1);
            var timestamp = new DateTime(reader.GetInt64(2));

            var metadata = new SnapshotMetadata(persistenceId, sequenceNr, timestamp);
            var snapshot = GetSnapshot(reader);

            return new SelectedSnapshot(metadata, snapshot);
        }
    }
}