using Microsoft.Data.SqlClient;
using dapper_orm.Models;
using Dapper;
using System.Data;

namespace dapper_orm
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            // string de conexão com banco de dados!
            const string connectionString = "Server=localhost,1433;Database=local;User ID=admin;Password=adminadmin;Trusted_Connection=False; TrustServerCertificate=True;";

            // abrindo conexão com o banco para querys;
            using (var connection = new SqlConnection(connectionString))
            {
                // executandos métodos com inserções do banco;
                // CreateManyCategory(connection);
                // CreateCategory(connection);
                // UpdateCategory(connection);
                // ExecuteProcedure(connection);
                // ExecuteReadProcedure(connection);
                // DeleteCategory(connection);
                // ExecuteScalar(connection);
                // ReadView(connection);
                // OneToOne(connection);
                // OneToMany(connection);
                // QueryMultiple(connection); // => ManyToMany
                // SelectIn(connection);
                // Like(connection, "api");
                Transaction(connection);
                ListCategories(connection);
            }
        }
        // listando categorias com query não anonimo (dinamic)
        static void ListCategories(SqlConnection connection)
        {
            var categories = connection.Query<Category>($"SELECT [Id], [Title] FROM [Category]");
            foreach (var item in categories)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }               
        }
        // criando varias categorias com um array de objeto anonimo
        static void CreateManyCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon-aws";
            category.Description = "Categorias destinadas a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var category2 = new Category();
            category2.Id = Guid.NewGuid();
            category2.Title = "Categoria Nova";
            category2.Url = "categoria-nova";
            category2.Description = "Categoria Nova";
            category2.Order = 9;
            category2.Summary = "Categoria";
            category2.Featured = true;

            // nunca concatenar!! para previnir SQL Injection! ex:  category.Url = "'SELECT...'";
            var insertSql = @"INSERT INTO 
            [Category] VALUES(
                @Id, 
                @Title, 
                @Url, 
                @Summary, 
                @Order, 
                @Description, 
                @Featured
            )";
            
            // um array de objetos anonimos para criar mais de uma;
            var rows = connection.Execute(insertSql, new[]{
            new {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            },
            new {
                category2.Id,
                category2.Title,
                category2.Url,
                category2.Summary,
                category2.Order,
                category2.Description,
                category2.Featured
            }
            });
            Console.WriteLine($"Linhas inseridas {rows}");
        }
        // deletando uma categoria
        static void DeleteCategory(SqlConnection connection)
        {
            var deleteQuery = "DELETE [Category] WHERE [Id]=@id";
            var rows = connection.Execute(deleteQuery, new
            {
                id = new Guid("89f4d69f-48c9-40a7-91d5-c0158dd129c1"),
            });

            Console.WriteLine($"{rows} registros excluídos");
        }
        // criando uma categoria com um objeto anonimo
        static void CreateCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"INSERT INTO 
                    [Category] 
                VALUES(
                    @Id, 
                    @Title, 
                    @Url, 
                    @Summary, 
                    @Order, 
                    @Description, 
                    @Featured)";

            var rows = connection.Execute(insertSql, new
            {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });
            Console.WriteLine($"{rows} linhas inseridas");
        }
        // att uma categoria
        static void UpdateCategory(SqlConnection connection)
        {
            var updatequery = "UPDATE [Category] SET [Title]=@title WHERE [Id]=@id";
            var rows = connection.Execute(updatequery, new {
                id = new Guid("b4c5af73-7e02-4ff7-951c-f69ee1729cac"),
                title = "Cloud"
            });

            Console.WriteLine($"Linhas atualizadas: {rows}");
        }

        static void ExecuteProcedure(SqlConnection connection)
        {
            var procedure = "[spDeleteStudent]";
            var pars = new { StudentId = "ff208e00-776b-45e9-8bfe-c78de857f082"};
            var affectedRows = connection.Execute(procedure, pars,commandType: CommandType.StoredProcedure);
            Console.WriteLine("Linhas Afetadas: " + affectedRows);
        }
        // executando com query anônima <>
        static void ExecuteReadProcedure(SqlConnection connection)
        {
            var procedure = "[spGetCoursesByCategory]";
            var pars = new { CategoryId = "09ce0b7b-cfca-497b-92c0-3290ad9d5142"};
            var courses = connection.Query(procedure, pars,commandType: CommandType.StoredProcedure);
            foreach (var item in courses)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }
        // executando com query anônima <>
        static void ExecuteScalar(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS 2022";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 10;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"INSERT INTO [Category] 
                OUTPUT inserted.[Id]
                VALUES(
                    NEWID(),
                    @Title, 
                    @Url, 
                    @Summary, 
                    @Order, 
                    @Description, 
                    @Featured
                )";

            var id = connection.ExecuteScalar<Guid>(insertSql, new
            {
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });
            Console.WriteLine($"Guid da categoria inserida:\n{id}");
        }
        // vizualizando view
        static void ReadView(SqlConnection connection)
        {
            var sql = "SELECT * FROM vwCourses";

            var courses = connection.Query(sql);
            foreach (var course in courses)
            {
                Console.WriteLine($"{course.Id} - {course.Title}");
            }
        }
        // relacionando item e separando com splitOn
        static void OneToOne(SqlConnection connection)
        {
            var sql = @"
            SELECT 
                *
            FROM
                [CareerItem] INNER JOIN [Course]
            ON 
                [CareerItem].[CourseId] = [Course].[Id]
            ";
            var items = connection.Query<CareerItem, Course, CareerItem>(sql, (careerItem, course) => {
                careerItem.Course = course;
                return careerItem;
            },splitOn: "Id");

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Title} - Curso: {item.Course.Title}");
            }
        }

        static void OneToMany(SqlConnection connection)
        {
            var sql = @"
                SELECT 
                    [Career].[Id],
                    [Career].[Title],
                    [CareerItem].[CareerId],
                    [CareerItem].[Title]
                FROM 
                    [Career] 
                INNER JOIN 
                    [CareerItem] ON [CareerItem].[CareerId] = [Career].[Id]
                ORDER BY
                    [Career].[Title]";

            var careers = new List<Career>();
            var items = connection.Query<Career, CareerItem, Career>(
                sql,
                (career, item) =>
                {
                    var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
                    if (car == null)
                    {
                        car = career;
                        car.Items.Add(item);
                        careers.Add(car);
                    }
                    else
                    {
                        car.Items.Add(item);
                    }

                    return career;
                }, splitOn: "CareerId");

            foreach (var career in careers)
            {
                Console.WriteLine($"{career.Title}");
                foreach (var item in career.Items)
                {
                    Console.WriteLine($" - {item.Title}");
                }
            }
        }
        // multipla querys
        static void QueryMultiple(SqlConnection connection)
        {
            var query = "SELECT * FROM [Category]; SELECT * FROM [Course]";
            
            using (var multi = connection.QueryMultiple(query))
            {
                var categories = multi.Read<Category>();
                var courses = multi.Read<Course>();

                foreach (var category in categories)
                {
                    Console.WriteLine($"{category.Title}");
                }

                foreach (var course in courses)
                {
                    Console.WriteLine($"{course.Title}");
                }
            }
        }
        // select in
        static void SelectIn(SqlConnection connection)
        {
            /* sem parametros */
            // var query = @"
            // SELECT * FROM [Career] WHERE [Id] IN(
            //     '09ce0b7b-cfca-497b-92c0-3290ad9d5142',
            //     '6cd9ba03-5521-43fa-8275-553fd5ca042a'
            // )";
            // var items = connection.Query<Career>(query);

            var query = "SELECT * FROM [Career] WHERE [Id] IN @Id";
            var items = connection.Query<Career>(query, new {
                Id = new[] {
                    "4327ac7e-963b-4893-9f31-9a3b28a4e72b",
                    "01ae8a85-b4e8-4194-a0f1-1c6190af54cb"
                }
            });
            foreach (var item in items)
            {
                Console.WriteLine($"- {item.Title}");
            }
        }
        // like com parametro no método
        static void Like(SqlConnection connection, string term)
        {
            var query = "SELECT * FROM [Course] WHERE [Title] LIKE @exp";
            var items = connection.Query<Course>(query, new {
                exp = $"%{term}%"
            });

            foreach (var item in items)
            {
                Console.WriteLine("- " + item.Title);
            }
        }

        static void Transaction(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Testi";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"
            INSERT INTO 
                [Category] 
            VALUES(
                @Id, @Title, 
                @Url, @Summary, 
                @Order, @Description, 
                @Featured
            )";

            connection.Open();
            using(var transaction = connection.BeginTransaction())
            {
                var rows = connection.Execute(insertSql, new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured
                }, transaction);

                // transaction.Commit();
                transaction.Rollback();

                Console.WriteLine($"Linhas Inseridas: {rows}");
            }
        }
    } 
}