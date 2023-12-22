using DesafioFinal.BancoDeDados;
using DesafioFinal.BancoDeDados.DTOs;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

namespace DesafioFinal
{
    public static class TopClientes
    {

        public static void MapPedidosEndPoint(this WebApplication app)
        {

            _ = app.MapPost("/pedidos/resumo", async (InMemoryContext context) =>
            {
                var consultaPedido =
                from pedidos in context.Pedidos
                orderby pedidos.order_date
                select pedidos;


                var Mes1 = consultaPedido.Where(x => x.order_date.Month == 9);
                var Mes2 = consultaPedido.Where(x => x.order_date.Month == 10);
                var Mes3 = consultaPedido.Where(x => x.order_date.Month == 11);
                var Mes4 = consultaPedido.Where(x => x.order_date.Month == 12);


                var consultaTopClientes =
                from Clientes in context.Clientes
                select Clientes;


                Dictionary<string, double> ListaCompleta = new Dictionary<string, double>();
                List<double> ListaSomas = new List<double>();

                Dictionary<int, double> ListaQuinzenas1 = new Dictionary<int, double>();
                Dictionary<int, double> ListaQuinzenas2 = new Dictionary<int, double>();

                for (var i = 0; i < consultaTopClientes.Count(); i++)
                {
                    var valores = consultaPedido.Where(x => x.customer_id == consultaTopClientes.ToList()[i].customer_id);

                    ListaCompleta.Add(consultaTopClientes.ToList()[i].first_name + " " + consultaTopClientes.ToList()[i].last_name + ": ", Math.Round(valores.Sum(x => x.total_amount), 2));

                }


                for (var i = 10; i <= 12; i++)
                {
                    var valores = consultaPedido.Where(x => x.order_date.Month == i);

                    if (valores != null)
                    {
                        ListaQuinzenas1.Add(i, valores.Where(x => x.order_date.Day <= 15).Sum(x => Math.Round(x.total_amount, 2)));

                        ListaQuinzenas2.Add(i, valores.Where(x => x.order_date.Day > 15).Sum(x => Math.Round(x.total_amount, 2)));
                    }
                }





                return new
                {
                    totalpedidos = new
                    {
                        Setembro = Math.Round(Mes1.Sum(x => x.total_amount), 2),
                        Outubro = Math.Round(Mes2.Sum(x => x.total_amount), 2),
                        Novembro = Math.Round(Mes3.Sum(x => x.total_amount), 2),
                        Dezembro = Math.Round(Mes4.Sum(x => x.total_amount), 2),
                    },

                    topclientes = ListaCompleta.Take(10).OrderByDescending(x => x.Value),

                    totalporquinzena = new
                    {
                        Quinzena1 = ListaQuinzenas1,
                        Quinzena2 = ListaQuinzenas2,
                    }
                };
            });
        }
    }


    public static class TopPedidos
    {
        public static void MapPedidos2EndPoint(this WebApplication app)
        {

            app.MapPost("/pedidos/mais_comprados", async (InMemoryContext context) =>
            {

                var ConsultaItens =
                from ItensDePedidos in context.ItensDePedidos
                select ItensDePedidos;

                var ConsultaProdutos =
                from Produtos in context.Produtos
                select Produtos;

                var ConsultaCategorias =
                from Categorias in context.Categorias
                select Categorias;



                var ListaCompleta = from Produtos in ConsultaProdutos
                                    join Itens in ConsultaItens on Produtos.category_id equals Itens.product_id
                                    select new
                                    {
                                        Nome = Produtos.product_name,
                                        Categoria = Produtos.category_id,
                                        Quantidade = Itens.quantity,
                                        Valor = Itens.price,
                                    };


                var ListaValor = ListaCompleta.OrderByDescending(x => x.Valor).ToList();
                var ListaQuantidade = ListaCompleta.OrderByDescending(x => x.Quantidade).ToList();


                return new
                {
                    produtosMaisCompradosPorQuantidade = ListaQuantidade.Take(30),
                    produtosMaisCompradosPorValor = ListaValor.Take(30)

                };
            });
        }
    }


    public static class TopCategoria
    {
        public static void MapPedidos3EndPoint(this WebApplication app)
        {

            app.MapPost("/pedidos/mais_comprados_por_categoria", async (InMemoryContext context) =>
            {
                var ConsultaItens =
                from ItensDePedidos in context.ItensDePedidos
                select ItensDePedidos;

                var ConsultaProdutos =
                from Produtos in context.Produtos
                select Produtos;

                var ConsultaCategorias =
                from Categorias in context.Categorias
                select Categorias;

                var ListaCompleta = from Produtos in ConsultaProdutos
                                    join Itens in ConsultaItens on Produtos.product_id equals Itens.product_id
                                    select new
                                    {
                                        Nome = Produtos.product_name,
                                        Categoria = Produtos.category_id,
                                        Quantidade = Itens.quantity,
                                        Valor = Itens.price,
                                    };

                var ListaCategoriaValor = ListaCompleta.GroupBy(x => x.Categoria).Select(x => new
                {
                    Categoria = x.Key,
                    Produtos = x.Select(x => new
                    {
                        Nome = x.Nome,
                        Quantidade = x.Quantidade,
                        Valor = x.Valor
                    }).OrderByDescending(x => x.Valor).Take(30).ToList(),
                }).OrderBy(x => x.Categoria);

                var ListaCategoriaQuantidade = ListaCompleta.GroupBy(x => x.Categoria).Select(x => new
                {
                    Categoria = x.Key,
                    Produtos = x.Select(x => new
                    {
                        Nome = x.Nome,
                        Quantidade = x.Quantidade,
                        Valor = x.Valor
                    }).OrderByDescending(x => x.Quantidade).Take(30).ToList(),
                }).OrderBy(x => x.Categoria);


                return new
                {
                    nomeDaCategoriaPorValor = ListaCategoriaValor,
                    nomeDaCategoriaPorQuantidade = ListaCategoriaQuantidade,
                };




            });


        }
    }

    public static class TopFornecedor
    {
        public static void MapPedidos4EndPoint(this WebApplication app)
        {

            app.MapPost("/pedidos/mais_comprados_por_fornecedor", async (InMemoryContext context) =>
            {
                var ConsultaFornecedores =
                from Fornecedores in context.Fornecedores
                select Fornecedores;

                var ConsultaProdutos = 
                from Produtos in context.Produtos
                select Produtos;

                var ConsultaItens = 
                from Itens in context.ItensDePedidos
                select Itens;

                var ListaCompleta =
                from Produtos in ConsultaProdutos
                join Fornecedores in ConsultaFornecedores on Produtos.supplier_id equals Fornecedores.supplier_id
                join Itens in ConsultaItens on Fornecedores.supplier_id equals Itens.order_id
                select new
                {
                    Nome = Fornecedores.supplier_name,
                    NomeProduto = Produtos.product_name,
                    Categoria = Produtos.category_id,
                    Quantidade = Itens.quantity,
                    Valor = Produtos.price
                };

                var ListaFornecedores = ListaCompleta.GroupBy(x => x.Nome).Select(x => new
                {
                    Nome = x.Key,
                    Produtos = x.Select(x => new
                    {
                        Nome = x.NomeProduto,
                        Valor = x.Valor,
                        Quantidade = x.Quantidade,
                        Categoria = x.Categoria,

                    }).OrderByDescending(x => x.Quantidade).Take(30).ToList()
                }).OrderBy(x => x.Nome) ;


                return new
                {
                    ItensMaisComprados = ListaFornecedores
                };

            });
        }
    }
}


