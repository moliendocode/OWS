﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using System.Threading.Tasks;
using Dapper.Transaction;
using Microsoft.Extensions.Options;
using OWSData.Models;
using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSData.Repositories.Interfaces;
using OWSData.Models.Tables;
using OWSData.SQL;
using OWSShared.Options;

namespace OWSData.Repositories.Implementations.Postgres
{
    public class GlobalDataRepository : IGlobalDataRepository
    {
        private readonly IOptions<StorageOptions> _storageOptions;

        public GlobalDataRepository(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions;
        }

        private IDbConnection Connection => new NpgsqlConnection(_storageOptions.Value.OWSDBConnectionString);

        public async Task AddOrUpdateGlobalData(GlobalData globalData)
        {
            using (var connection = Connection as NpgsqlConnection)
            {
                if (connection == null)
                {
                    throw new InvalidOperationException("Connection is not an NpgsqlConnection");
                }

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var outputGlobalData = await connection.QuerySingleOrDefaultAsync<GlobalData>(GenericQueries.GetGlobalDataByGlobalDataKey,
                    globalData,
                    commandType: CommandType.Text);

                if (outputGlobalData != null)
                {
                    await connection.ExecuteAsync(GenericQueries.UpdateGlobalData,
                        globalData,
                        commandType: CommandType.Text);
                }
                else
                {
                    await connection.ExecuteAsync(GenericQueries.AddGlobalData,
                        globalData,
                        commandType: CommandType.Text);
                }
            }
        }

        public async Task<GlobalData> GetGlobalDataByGlobalDataKey(Guid customerGuid, string globalDataKey)
        {
            using (Connection)
            {
                var parameters = new
                {
                    CustomerGUID = customerGuid,
                    GlobalDataKey = globalDataKey
                };

                return await Connection.QueryFirstOrDefaultAsync<GlobalData>(GenericQueries.GetGlobalDataByGlobalDataKey, parameters);
            }
        }
    }
}
