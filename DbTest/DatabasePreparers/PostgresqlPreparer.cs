﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DbTest
{
    public class PostgresqlPreparer : IDatabasePreparer
    {
        public void BeforeLoad(IDataAccessLayer connection)
        {
            connection.Execute(@"
                    do
                    $$
                    declare
                      l_stmt text;
                    begin
                      select 'truncate ' || string_agg(format('%I.%I', schemaname, tablename), ',')
                        into l_stmt
                      from pg_tables
                      where schemaname in ('public') and tablename != '__EFMigrationsHistory';

                      execute l_stmt;
                    end;
                    $$
                ");
        }

        public void AfterLoad(IDataAccessLayer connection)
        {
        }

        public void InsertObjects(IDataAccessLayer connection, string tableName, List<string> columnNames, List<object[]> rows)
        {
            var columns = string.Join(",", columnNames.Select(x => $"\"{x}\""));

            foreach (var row in rows)
            {
                var values = new List<string>();
                foreach (var val in row)
                { 
                    switch(val)
                    {
                        case null:
                            values.Add("null");
                            break;
                        case String str:
                            values.Add($"'{str}'");
                            break;
                        case Guid guid:
                            values.Add($"'{guid}'");
                            break;
                        case double d:
                            values.Add(d.ToString(CultureInfo.InvariantCulture));
                            break;
                        case decimal d:
                            values.Add(d.ToString(CultureInfo.InvariantCulture));
                            break;
                        case float f:
                            values.Add(f.ToString(CultureInfo.InvariantCulture));
                            break;
                        case bool boolVal:
                            values.Add(boolVal ? "True" : "False");
                            break;
                        case DateTime dateVal:
                            values.Add($"'{dateVal.ToString("yyyy-MM-dd HH:mm:ss")}'");
                            break;
                        case int i:
                            values.Add(i.ToString());
                            break;
                        default:
                            var valType = val.GetType();

                            if (valType.GetTypeInfo().IsEnum)
                            {
                                var enumVal = (int)val;
                                values.Add(enumVal.ToString());
                            }
                            else
                            {
                                values.Add(val.ToString());
                            }
                            break;
                    }
                    
                }

                var sql = $@"INSERT INTO ""{tableName}"" ({columns}) VALUES ({string.Join(",", values)});";
                connection.Execute(sql);
            }
        }
    }
}
