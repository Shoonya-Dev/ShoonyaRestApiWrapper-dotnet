using NorenRestApiWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace NorenApiWrapper
{
    public static class NorenApiHelpers
    {
        public static DataTable GetHoldingsDataTable(List<HoldingsItem> list)
        {
            DataTable dataTable = new DataTable(typeof(HoldingsItem).Name);

            //Get all the properties
            FieldInfo[] Fields = typeof(HoldingsItem).GetFields();
            FieldInfo[] ScripFields = typeof(ScripItem).GetFields();

            foreach (FieldInfo field in Fields)
            {
                if (field.Name == "exch_tsym")
                {//ignore the list
                    continue;
                }
                //Setting column names as Property names
                dataTable.Columns.Add(field.Name);
            }
            //add token columns for nse and bse
            //embedded list add the columns            
            {
                //primary
                dataTable.Columns.Add("exch_nse");
                dataTable.Columns.Add("ls_nse");
                dataTable.Columns.Add("pp_nse");
                dataTable.Columns.Add("ti_nse");
                dataTable.Columns.Add("token_nse");
                dataTable.Columns.Add("tsym_nse");
            }


            {
                //secondary
                dataTable.Columns.Add("exch_bse");
                dataTable.Columns.Add("ls_bse");
                dataTable.Columns.Add("pp_bse");
                dataTable.Columns.Add("ti_bse");
                dataTable.Columns.Add("token_bse");
                dataTable.Columns.Add("tsym_bse");
            }


            foreach (HoldingsItem item in list)
            {
                var values = new object[dataTable.Columns.Count];
                //i is our index for columns in datatable for values 
                int i = 0;
                for (int iField = 0; iField < Fields.Length; iField++)
                {
                    if (Fields[iField].Name == "exch_tsym")
                    {//ignore the list                        
                        continue;
                    }
                    //inserting property values to datatable rows
                    values[i++] = Fields[iField].GetValue(item);
                }
                //add the first scrips

                bool nse = false;
                foreach(var exch_tsym in item.exch_tsym)
                {
                    //first add nse then bse
                    if (exch_tsym.exch != "NSE")
                        continue;                     
                    values[i++] = exch_tsym.exch;
                    values[i++] = exch_tsym.ls;
                    values[i++] = exch_tsym.pp;
                    values[i++] = exch_tsym.ti;
                    values[i++] = exch_tsym.token;
                    values[i++] = exch_tsym.tsym;
                    nse = true;
                }

                //move index to bse columns if nse isnt there
                if (nse == false)
                    i = i + 6;

                foreach (var exch_tsym in item.exch_tsym)
                {
                    if (exch_tsym.exch != "BSE")
                        continue;
                    values[i++] = exch_tsym.exch;
                    values[i++] = exch_tsym.ls;
                    values[i++] = exch_tsym.pp;
                    values[i++] = exch_tsym.ti;
                    values[i++] = exch_tsym.token;
                    values[i++] = exch_tsym.tsym;
                }                

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}
