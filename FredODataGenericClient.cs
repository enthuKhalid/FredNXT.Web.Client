using System;
using System.Data.Services.Client;

namespace FredNXT.Web.Client
{
    /// <summary>
    /// An Odata client implementation to dynamically list all properties within any given object identified by its Type(T)
    /// </summary>
    /// <typeparam name="T">The Odata entity type</typeparam>
    public class FredData<T>
    {
        /// <summary>
        /// gets the (top 3) items under the specified entity and displays its propert values
        /// </summary>
        /// <param name="ctx">The data service context</param>
        /// <param name="entityName">The name of entity to retrieve</param>
        public void Get(DataServiceContext ctx, string entityName)
        {
            //dynamic queries can be generated using the dataservice context
            //and query options can be appended for more control on how you want to execute it.
            var query = ctx.CreateQuery<T>(entityName).AddQueryOption("$top", 3);

            //run the above created query, and loop through the results
            var result = query.Execute();

            foreach (var item in result)
            {
                DisplayItem(item);
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// a generics method that will enumeate the properties within the given object 
        /// </summary>
        /// <param name="item">an OData row</param>
        private void DisplayItem(T item)
        {
            foreach (var prop in item.GetType().GetProperties())
            {
                Console.WriteLine("    {0}={1}", prop.Name, prop.GetValue(item));
            }
        }
    }
}
