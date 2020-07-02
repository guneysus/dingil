using CommandLine;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Dingil;
using LiteDB;
using System.IO;
using Dingoz.Service;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace Dingoz.Service
{
    public class ApiController<T>
    {
        private readonly LiteDatabase db;
        private readonly ILiteCollection<T> collection;

        public ApiController(LiteDatabase db)
        {
            this.db = db;
            this.collection = db.GetCollection<T>();
        }

        public async Task GetAll(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            var items = collection.FindAll();

            await context.Response.WriteJsonAsync(new
            {
                errors = new string[] { },
                items = items
            });
        }

        public async Task GetById(HttpContext context)
        {
            (int id, bool ok) = context.Request.RouteValues.Get<int>("id");

            var response = collection.FindById(id);
            if (response == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteJsonAsync(new
                {
                    items = new object[] { },
                    errors = new string[] { }
                });

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteJsonAsync(new
            {
                items = new object[] { response },
                errors = new string[] { }
            });

        }

        public async Task Post(HttpContext context)
        {
            var entity = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(context.Request.BodyReader.AsStream());

            var result = collection.Insert(entity);

            await context.Response.WriteJsonAsync(new
            {
                items = new object[] { collection.FindById(result) },
                errors = new string[] { }
            });

        }

        public async Task Put(HttpContext context)
        {
            (int id, bool ok) = context.Request.RouteValues.Get<int>("id");

            var entity = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(context.Request.BodyReader.AsStream());

            var result = collection.Update(id, entity);

            if (!result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteJsonAsync(new
                {
                    result = result,
                    errors = new string[] { }
                });

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;

            await context.Response.WriteJsonAsync(new
            {
                result = result,
                errors = new string[] { }
            });
        }

        public async Task Patch(HttpContext context, T entity) => throw new NotImplementedException();
        public async Task Delete(HttpContext context, int id) => throw new NotImplementedException();
    }

}
