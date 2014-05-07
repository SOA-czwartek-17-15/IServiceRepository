using NServiceRepository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServer.Models
{
    class Repository
    {
        private EFDbContext context;
        private IEnumerable<Service> Services{
            get { return context.Servs; }
        }
        public Repository() {
            context = new EFDbContext();
            KillZombieServices();
            //CleanServices();
        }
        public void AddService(Service serv){
            context.Servs.Add(serv);
            context.SaveChanges();
        }
        public void RemoveService(Service serv)
        {
            context.Servs.Remove(serv);
            context.SaveChanges();
        }
        public void UpdateService(Service serv)
        {
            using (var dbCtx = new EFDbContext())
            {
                dbCtx.Entry(serv).State = EntityState.Modified;    
                dbCtx.SaveChanges();
            }
        }
        public Service FindService(String Name)
        {
            return context.Servs.SingleOrDefault(serv => serv.Name == Name);
        }
        public void KillZombieServices()
        {
            TimeSpan duration;
            foreach (var serv in context.Servs)
            {
                duration = DateTime.Now - serv.LastSeen;
                if (duration.Seconds > 5)
                {
                    context.Servs.Remove(serv);
                    Console.Write("Serwis {0} wygasł", serv.Name);
                }     
            }
            context.SaveChanges();
        }
        public void CleanServices()
        {
            foreach (var serv in context.Servs)
                context.Servs.Remove(serv);
            context.SaveChanges();
        }

    }
}
