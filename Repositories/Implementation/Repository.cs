﻿using Microsoft.EntityFrameworkCore;
using PBL3.Models.Domain;
using PBL3.Repositories.Abstract;

namespace PBL3.Repositories.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        // 1 DbContext sẽ được tạo cho mỗi Request
        protected readonly AppDbContext context;
        protected readonly DbSet<T> table;
        public Repository(AppDbContext context)
        {
            this.context = context;
            this.table = context.Set<T>();
        }
        public IQueryable<T> AsQueryable() => table.AsQueryable();
        public virtual IQueryable<T> GetAll()
        {
            return table;
        }

        public virtual T? GetById(int id)
        {
            return table.Find(id);
        }

        public virtual void Add(T obj)
        {
            table.Add(obj);
            context.SaveChanges();
        }

        public virtual void Update(T obj)
        {
            table.Update(obj);
			context.SaveChanges();
        }
        public virtual void Delete(int id)
        {
            T? obj = table.Find(id);
            if (obj != null)
            {
                table.Remove(obj);
                context.SaveChanges();
            }
        }
        public virtual void LogChangeTracker()
        {
			context.ChangeTracker.DetectChanges();
            Console.WriteLine("Tracked entities at current request : ");
			Console.WriteLine(context.ChangeTracker.DebugView.LongView);
		}
    }
}
