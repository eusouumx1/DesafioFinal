using DesafioFinal.BancoDeDados;
using DesafioFinal.BancoDeDados.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DesafioFinal
{
    public static class NomeClientes
    {
        public static void MapClientesEndPoint(this WebApplication app)
        {
            app.MapPost("/clientes", async (InMemoryContext context) =>
            {
                IQueryable<Clientes> consultaDominio =
                    from cliente in context.Clientes
                    orderby cliente.first_name
                    select cliente;

                List<string> clientesNome = new List<string>(100);
                List<string> clientesEmail = new List<string>(100);
                foreach (var item in consultaDominio)
                {
                    string fullName = item.first_name + " " + item.last_name;
                    clientesNome.Add(fullName);
                    clientesEmail.Add(item.email);
                }

                return new
                {
                    Nome_Completo = clientesNome.Take(100),
                    Email = clientesEmail.Take(100)
                };
            });

        }

    }



    public static class PaisDominio
    {
        public static void MapPaisesEndPoint(this WebApplication app)
        {
            app.MapPost("/clientes/resumo", async (InMemoryContext context) => {
                var consultaPais =
                from cliente in context.Clientes
                group cliente by cliente.country into countrygroup
                select new
                {
                    Country = countrygroup.Key,
                    Group = countrygroup.Count()
                };



                var consultaDominio =
                from cliente in context.Clientes
                group cliente by cliente.email.Substring(cliente.email.IndexOf("@") + 1) into emailgroup
                select new
                {
                    Country = emailgroup.Key,
                    Group = emailgroup.Count()
                };

                consultaDominio = consultaDominio.OrderByDescending(x => x.Group);


                return new
                {
                    paisesComMaisClientes = consultaPais.Take(5),

                    dominios = consultaDominio.Take(5)
                };
            });


        }
    }



}
