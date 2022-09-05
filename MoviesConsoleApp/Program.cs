﻿using Microsoft.EntityFrameworkCore;
using Persistencia.Entidades;
using Persistencia.Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoviesConsoleApp
{
    class Program
    {
        static void Main(String[] args)
        {
            // acesso ao EF serah realizado pela variavel _db
            // essa dependencia da camada de apresentacao com
            // a camada de persistencia eh errada!
            MovieContext _db = new MovieContext();

            Console.WriteLine();
            Console.WriteLine("1. Listar o nome de todos personagens desempenhados por um determinado ator, incluindo a informação de qual o título do filme e o diretor");
            var consulta1 = from character in _db.Characters
                            .Include(c => c.Actor)
                            .Include(c => c.Movie)
                        where character.Actor.Name == "Tom Hanks"
                        select new {
                            Personagem = character.Character,
                            TituloFilme = character.Movie.Title,
                            Diretor = character.Movie.Director
                        };
            foreach (var x in consulta1) {
                Console.WriteLine(String.Format("{0} - {1} - {2}", x.Personagem, x.TituloFilme, x.Diretor));
            }

            Console.WriteLine();
            Console.WriteLine("2. Mostrar o nome e idade de todos atores que desempenharam um determinado personagem(por exemplo, quais os atores que já atuaram como '007' ?");
            var consulta2 = from character in _db.Characters
                            .Include(c => c.Actor)
                            where character.Character == "James Bond"
                            select new {
                                Nome = character.Actor.Name,                                
                                Idade = character.Actor.DateBirth > DateTime.Now ? DateTime.Now.Year - character.Actor.DateBirth.Year : (DateTime.Now.Year - character.Actor.DateBirth.Year) - 1,                           
                                Personagem = character.Character,
                            };
            foreach (var x in consulta2.Distinct()) {
                Console.WriteLine(String.Format("{0} - {1}", x.Nome, x.Idade));
            }

            Console.WriteLine();
            Console.WriteLine("3. Informar qual o ator desempenhou mais vezes um determinado personagem(por exemplo: qual o ator que realizou mais filmes como o 'agente 007'");
            var personagem = "James Bond";
            var consulta3 = from character in _db.Characters
                            .Include(c => c.Actor)
                            where character.Character == personagem
                            select new {
                                Nome = character.Actor.Name                               
                            };
            var agrupamento3 = from it in consulta3
                           group it by it.Nome into grp
                           select new {
                               Nome = grp.Key,
                               Quant = grp.Count()
                           };
            var maisVezes = 0;
            foreach (var res in agrupamento3) {
                if (maisVezes < res.Quant) {
                    personagem = res.Nome;
                    maisVezes = res.Quant;
                }
            }
            Console.WriteLine("{0}", personagem);

            Console.WriteLine();
            Console.WriteLine("4. Mostrar o nome e a data de nascimento do ator mais idoso");
            var consulta4 = from actor in _db.Actors                      
                            select new {
                                Nome = actor.Name,
                                Idade = actor.DateBirth
                            };

            var consultaOrdenada4 = consulta4.OrderBy(x => x.Idade).FirstOrDefault();
            Console.WriteLine("{0} - {1}", consultaOrdenada4.Nome, consultaOrdenada4.Idade);

            Console.WriteLine();
            Console.WriteLine("5. Mostrar o nome e a data de nascimento do ator mais novo a atuar em um determinado gênero");
            var consulta5 = from character in _db.Characters
                            .Include(c => c.Movie)
                                .ThenInclude(c => c.Genre)
                                .Include("Actor")
                            where character.Movie.Genre.Name.Equals("Action")
                            select new {
                                Nome = character.Actor.Name,
                                Idade = character.Actor.DateBirth
                            };
            var consultaOrdenada5 = consulta5.OrderBy(x => x.Idade).FirstOrDefault();
            Console.WriteLine("{0} - {1}", consultaOrdenada5.Nome, consultaOrdenada5.Idade);

            Console.WriteLine();
            Console.WriteLine("6. Mostrar o valor médio das avaliações dos filmes de um determinado diretor");
            var consulta6 = (from movie in _db.Movies
                             where movie.Director == "Steven Spielberg"
                             select new {
                                 ValorAvaliacoes = movie.Rating
                             }).ToList();
            var soma = consulta6.Select(x => x.ValorAvaliacoes).Sum();
            var quantidade = consulta6.Select(x => x.ValorAvaliacoes).Count();
            Console.WriteLine("{0}", soma/quantidade);

            Console.WriteLine();
            Console.WriteLine("7. Qual o elenco do filme melhor avaliado ?");
            var consulta7 = _db.Movies                  
                            .Include(c => c.Characters)
                                .ThenInclude(c => c.Actor)
                            .OrderByDescending(x => x.Rating)
                            .FirstOrDefault()
                            .Characters;

            foreach (var res in consulta7) {
                Console.WriteLine("{0}", res.Actor.Name);
            }

            Console.WriteLine();
            Console.WriteLine("8. Qual o elenco do filme com o maior faturamento?");
            var consulta8 = _db.Movies
                            .Include(c => c.Characters)
                                .ThenInclude(c => c.Actor)
                            .OrderByDescending(x => x.Gross)
                            .FirstOrDefault()
                            .Characters;

            foreach (var res in consulta8) {
                Console.WriteLine("{0}", res.Actor.Name);
            }
           
            Console.WriteLine("- - -   feito!  - - - ");
            Console.WriteLine();
        }

        static void Main_presencial(String[] args)
        {
            // acesso ao EF serah realizado pela variavel _db
            // essa dependencia da camada de apresentacao com
            // a camada de persistencia eh errada!
            MovieContext _db = new MovieContext();

            #region # LINQ - consultas

            Console.WriteLine();
            Console.WriteLine("1. Todos os filmes de acao");

            Console.WriteLine("1a. Modelo tradicional");
            List<Movie> filmes1a = new List<Movie>();
            foreach (Movie f in _db.Movies.Include("Genre"))
            {
                if (f.Genre.Name == "Action")
                    filmes1a.Add(f);
            }

            foreach (Movie filme in filmes1a)
            {
                Console.WriteLine("\t{0} - {1}", filme.Title, filme.ReleaseDate.Year);
            }

            Console.WriteLine("\n1b. Usando linq - query syntax");
            var filmes1b = from f in _db.Movies
                          where f.Genre.Name == "Action"
                          select f;
            foreach (Movie filme in filmes1b)
            {
                Console.WriteLine("\t{0} - {1}", filme.Title, filme.Director);
            }

            Console.WriteLine("\n1c. Usando linq - method syntax");
            var filmes1c = _db.Movies.Where(m => m.Genre.Name == "Action");
            foreach (Movie filme in filmes1c)
            {
                Console.WriteLine("\t{0}", filme.Title);
            }

 
            Console.WriteLine();
            Console.WriteLine("2. Todos os diretores de filmes do genero 'Action', com projecao");
            var filmes2 = from f in _db.Movies
                          where f.Genre.Name == "Action"
                          select f.Director;

            foreach (var nome in filmes2)
            {
                Console.WriteLine("\t{0}", nome);
            }

            Console.WriteLine();
            Console.WriteLine("3a. Todos os filmes de cada genero (query syntax):");
            var generosFilmes3a = from g in _db.Genres.Include(gen => gen.Movies)
                                select g;
            foreach (var gf in generosFilmes3a)
            {
                if (gf.Movies.Count > 0)
                {
                    Console.WriteLine("\nFilmes do genero: " + gf.Name);
                    foreach (var f in gf.Movies)
                    {
                        Console.WriteLine("\t{0} - {1}", f.Title, f.Rating);
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("3b. Todos os filmes de cada genero (method syntax):");

            var generosFilmes3b = _db.Genres.Include(gen => gen.Movies).ToList();

            foreach (Genre gf in generosFilmes3a)
            {
                if (gf.Movies.Count > 0)
                {
                    Console.WriteLine("\nFilmes do genero: " + gf.Name);
                    foreach (var f in gf.Movies)
                    {
                        Console.WriteLine("\t{0}", f.Title);
                    }
                }
            }


            Console.WriteLine();
            Console.WriteLine("4. Titulo e ano dos filmes do diretor Quentin Tarantino, com projcao em uma class anonima:");
            var tarantino = from f in _db.Movies
                            where f.Director == "Quentin Tarantino"
                             select new
                             {
                                 Ano = f.ReleaseDate.Year,
                                 f.Title
                             };

            foreach (var item in tarantino)
            {
                Console.WriteLine("{0} - {1}", item.Ano, item.Title);
            }

            Console.WriteLine();
            Console.WriteLine("5. Todos os gêneros ordenados pelo nome:");
            var q5 = _db.Genres.OrderByDescending(g => g.Name);
            foreach (var genero in q5)
            {
                Console.WriteLine("{0, 20}\t {1}", genero.Name, genero.Description.Substring(0, 30));
            }

            Console.WriteLine();
            Console.WriteLine("6. Numero de filmes agrupados pelo anos de lançamento:");
            var q6 = from f in _db.Movies
                     group f by f.ReleaseDate.Year into grupo
                     select new
                     {
                         Chave = grupo.Key,
                         NroFilmes = grupo.Count()
                     };

            foreach (var ano in q6.OrderByDescending(g => g.NroFilmes))
            {
                Console.WriteLine("Ano: {0}  Numero de filmes: {1}", ano.Chave, ano.NroFilmes);

            }

            Console.WriteLine();
            Console.WriteLine("7. Projeção do faturamento total, quantidade de filmes e avaliação média agrupadas por gênero:");
            var q7 = from f in _db.Movies
                     group f by f.Genre.Name into grpGen
                     select new
                     {
                         Categoria = grpGen.Key,
                         Faturamento = grpGen.Sum(e => e.Gross),
                         Avaliacao = grpGen.Average(e => e.Rating),
                         Quantidade = grpGen.Count()
                     };

            foreach (var genero in q7)
            {
                Console.WriteLine("Genero: {0}", genero.Categoria);
                Console.WriteLine("\tFaturamento total: {0}\n\t Avaliação média: {1}\n\tNumero de filmes: {2}",
                                genero.Faturamento, genero.Avaliacao, genero.Quantidade);
            }
            #endregion



        }

        static void Main_CRUd(string[] args)
        {
            Console.WriteLine("Hello World!");

            MovieContext _context = new MovieContext();

            Genre g1 = new Genre()
            {
                Name = "Comedia",
                Description = "Filmes de comedia"
            };

            Genre g2 = new Genre()
            {
                Name = "Ficcao",
                Description = "Filmes de ficcao"
            };

            _context.Genres.Add(g1);
            _context.Genres.Add(g2);

            _context.SaveChanges();

            List<Genre> genres = _context.Genres.ToList();

            foreach (Genre g in genres)
            {
                Console.WriteLine(String.Format("{0,2} {1,-10} {2}",
                                    g.GenreId, g.Name, g.Description));
            }

        }
    }
}