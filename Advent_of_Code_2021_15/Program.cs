using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace _21f_kupac_cs
{
    class Program
    {
        class Kupac<T>
        {
            // --- VÁLTOZÓK --- 

            public List<T> t;
            Func<T, T, int> comparator;

            // --- KONSTRUKTOROK ---

            /// <summary>
            /// Kupac adatszerkezet, mindig a megadott comparator szerint legkisebb elemet adja ki. 
            /// </summary>
            /// <param name="comparator">háromértékű rendezés</param>
            public Kupac(Func<T, T, int> comparator)
            {
                this.comparator = comparator;
                this.t = new List<T>();
            }

            public Kupac(Func<T, T, int> comparator, IEnumerable<T> kezdoertekek) : this(comparator)
            {
                foreach (T elem in kezdoertekek)
                    Push(elem);
            }


            // --- PUBLIKUS METÓDUSOK ---

            public int Count { get => t.Count; }
            public bool Empty() => t.Count == 0;
            public T Peek() => t[Elso];
            public void Push(T e)
            {
                t.Add(e);
                Fellebegtet(Utolso);
            }
            public T Pop()
            {
                T result = Peek();
                Csere(Elso, Utolso);
                t.RemoveAt(Utolso);
                Sullyeszt(Elso);
                return result;
            }
            public void Repair(T e)
            {
                List<int> elem_helyei = Tombkeres(e);
                foreach (int i in elem_helyei)
                    Megigazit(i);
            }
            public string ToGraphviz() => $"digraph{{\n {ToGraphviz(0)}\n}}\n";
            public void Diagnosztika() => Console.WriteLine(this.ToGraphviz());
            public void Keres_teszt()
            {
                Console.WriteLine("KERESÉS TESZT: MEGVIZSGÁLJUK, HOGY MINDEN ELEMRE UGYANAZT ADJA-E A KÉT KERESÉS!");
                foreach (T elem in t)
                {
                    List<int> elem_helyei = Tombkeres(elem);
                    int i = Keres(elem);
                    Console.WriteLine($"Az {elem} helye tömb szerint {string.Join(", ", elem_helyei)} és fa szerint {i} --> {(elem_helyei.Contains(i) ? "SZUPER" : "nemjó....")}");
                }
            }

            // --- TÖMBMETÓDUSOK ---

            int Elso { get => 0; }
            int Utolso { get => t.Count - 1; }
            void Csere(int i, int j) => (t[i], t[j]) = (t[j], t[i]);
            List<int> Tombkeres(T elem)
            {
                List<int> result = new List<int>();
                for (int i = 0; i < t.Count; i++)
                {
                    if (t[i].Equals(elem))
                        result.Add(i);
                }
                return result;
            }

            // --- TELJES BINÁRIS FA-METÓDUSOK ---

            int Szulo_indexe(int n) => n == 0 ? n : ((n + 1) / 2 - 1);
            List<int> Gyerekek_indexei(int n)
            {
                int kisebbik_indexe = 2 * n + 1;
                if (t.Count <= kisebbik_indexe)
                    return new List<int>();

                int nagyobbik_indexe = kisebbik_indexe + 1;
                if (t.Count <= nagyobbik_indexe)
                    return new List<int> { kisebbik_indexe };

                return new List<int> { kisebbik_indexe, nagyobbik_indexe };
            }
            int Kisebbik_gyerek_indexe(int n)
            {
                List<int> gyerekek = Gyerekek_indexei(n);
                switch (gyerekek.Count)
                {
                    case 0:
                        return -1;
                    case 1:
                        return gyerekek[0];
                    case 2:
                        return Kisebbik_ertek_indexe(gyerekek[0], gyerekek[1]);
                }
                throw new Exception("nem szabadna 2-nél több gyerek legyen!");
            }
            int Kisebbik_ertek_indexe(int a, int b) => comparator(t[a], t[b]) == -1 ? a : b;
            void Sullyeszt(int n)
            {
                int kgyi = Kisebbik_gyerek_indexe(n);
                if (kgyi == -1)
                    return;
                if (comparator(t[kgyi], t[n]) == -1)
                {
                    Csere(kgyi, n);
                    Sullyeszt(kgyi);
                }
            }
            void Fellebegtet(int n)
            {
                while (comparator(t[n], t[Szulo_indexe(n)]) == -1)
                {
                    Csere(n, Szulo_indexe(n));
                    n = Szulo_indexe(n);
                }
            }
            void Megigazit(int n)
            {
                Sullyeszt(n);
                Fellebegtet(n);
            }
            /// <summary>
            /// nagyon hatékonyan megkeresi egy elem helyét a fában, feltéve, hogy a kupacban nincsenek rendezési hibák!
            /// </summary>
            /// <param name="elem"></param>
            /// <returns></returns>
            int Keres(T elem) => Keres(0, elem);
            int Keres(int hol, T elem)
            {
                
                if (elem.Equals(t[hol]))
                    return hol;
                switch (comparator(elem, t[hol]))
                {
                    case -1:
                        return -1;
                    case 0:
                    default:

                        List<int> gyi = Gyerekek_indexei(hol);
                        switch (gyi.Count)
                        {
                            case 0:
                                return -1;
                            case 1:
                                return Keres(gyi[0], elem);
                            default:
                                int result = Keres(gyi[0], elem);
                                return result != -1 ? result : Keres(gyi[1], elem);
                        }
                }
            }
            string ToGraphvizNode(int i, T e) => $"    {i} [label=<{e}<SUB>{i}</SUB>>];\n";
            string ToGraphviz(int n)
            {
                List<int> gyi = Gyerekek_indexei(n);
                switch (gyi.Count)
                {
                    case 0:
                        return ToGraphvizNode(n, t[n]);
                    case 1:
                        return $"{ToGraphvizNode(n, t[n])}    {n} -> {gyi[0]};\n" + ToGraphviz(gyi[0]);
                    default:
                        return $"{ToGraphvizNode(n, t[n])}    {n} -> {gyi[0]};\n    {n} -> {gyi[1]};\n" + ToGraphviz(gyi[0]) + ToGraphviz(gyi[1]);
                }
            }
        }


        static void Main(string[] args)
        {
            (Dictionary<(int, int), int> m, (int, int) b) = Beolvasas();


            (Dictionary<(int, int), int> tav, Dictionary<(int, int), (int, int)> honnanvektor) = Dijkstra(m, (0, 0));


            List<(int, int)> fel = honnan_vektor_fel(honnanvektor, b);
            Console.WriteLine($"honnan vektor göngyölítve = {String.Join(", ", fel)}");

            fel.RemoveAt(0);

            Console.WriteLine($"sum: {fel.Sum(x => m[x])}");

            string[] st = File.ReadAllLines("map.txt");
            for (int i = 0; i < st.Length; i++)
            {
                for (int j = 0; j < st[0].Length; j++)
                {
                    (int, int) node = (i, j);
                    if (!fel.Contains(node))
                    {
                        Console.Write(st[i][j].ToString());
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.Write(st[i][j].ToString());
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }
                Console.Write("\n");
            }

            Console.ReadLine();
        }

        static List<(int, int)> honnan_vektor_fel(Dictionary<(int, int), (int, int)> honnan, (int, int) end)
        {
            List<(int, int)> result = new List<(int, int)>();

            (int, int) node = end;

            while (node != (-1, -1))
            {
                Console.WriteLine(node);
                result.Add(node);
                node = honnan[node];
            }

            result.Reverse();
            return result;
        }

        static (Dictionary<(int, int), int> m, (int, int) b) Beolvasas()
        {
            /*
1163751742
1381373672
2136511328
3694931569
7463417111
1319128137
1359912421
3125421639
1293138521
2311944581
            */

            Dictionary<(int x, int y), int> m = new Dictionary<(int, int), int>();
            string[] st = File.ReadAllLines("map.txt");

            for (int i = 0; i < st.Length; i++)
            {
                for (int j = 0; j < st[0].Length; j++)
                {
                    if (!m.ContainsKey((i, j)))
                    {
                        m.Add((i, j), int.Parse(st[i][j].ToString()));
                    }
                }
            }

            return (m, (st.Length - 1, st[0].Length - 1));
        }

        static List<(int, int)> Szomszédai(Dictionary<(int, int), int> m, (int, int) csucs)
        {
            List<(int, int)> szomszedok = new List<(int, int)>();

            // fel
            if (m.ContainsKey((csucs.Item1 - 1, csucs.Item2)))
            {
                szomszedok.Add((csucs.Item1 - 1, csucs.Item2));
            }

            // le
            if (m.ContainsKey((csucs.Item1 + 1, csucs.Item2)))
            {
                szomszedok.Add((csucs.Item1 + 1, csucs.Item2));
            }

            // jobbra
            if (m.ContainsKey((csucs.Item1, csucs.Item2 + 1)))
            {
                szomszedok.Add((csucs.Item1, csucs.Item2 + 1));
            }

            // balra
            if (m.ContainsKey((csucs.Item1, csucs.Item2 - 1)))
            {
                szomszedok.Add((csucs.Item1, csucs.Item2 - 1));
            }

            Console.WriteLine($"{csucs} szomszedai: {String.Join(", ", szomszedok)}");
            return szomszedok;
        }

        static int Plafonos_összeadás(int a, int b)
        {
            int c = a + b;
            if (0 < a && 0 < b && c < 0)
                return int.MaxValue;
            return c;
        }

        private static (Dictionary<(int, int), int>, Dictionary<(int, int), (int, int)>) Dijkstra(Dictionary<(int, int), int> m, (int, int) v)
        {
            Dictionary<(int, int), int> tav = new Dictionary<(int, int), int>();
            foreach ((int, int) k in m.Keys)
            {
                if (!tav.ContainsKey(k))
                {
                    tav.Add(k, int.MaxValue);
                }
            }
            tav[v] = 0;

            Kupac<(int, int)> tennivalok = new Kupac<(int, int)>(((int, int) x, (int, int) y) => tav[x].CompareTo(tav[y]));

            Dictionary<(int, int), (int, int)> honnan = new Dictionary<(int, int), (int, int)>();
            foreach ((int, int) k in m.Keys)
            {
                if (!honnan.ContainsKey(k))
                {
                    honnan.Add(k, (-2, -2));
                }
            }

            honnan[v] = (-1, -1);


            foreach ((int, int) k in m.Keys)
            {
                tennivalok.Push(k);
            }

            while (tennivalok.Count != 0)
            {
                (int, int) tennivalo = tennivalok.Pop();
                foreach (var szomszéd in Szomszédai(m, tennivalo))
                {
                    int új_jelölt = Plafonos_összeadás(tav[tennivalo], m[szomszéd]);
                    if (új_jelölt < tav[szomszéd])
                    {
                        tav[szomszéd] = új_jelölt;
                        tennivalok.Repair(szomszéd);
                        honnan[szomszéd] = tennivalo;
                    }
                }
            }

            return (tav, honnan);
        }
    }
}