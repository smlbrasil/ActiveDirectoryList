using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;
using System.Configuration;

namespace ActiveDirectoryList
{
    class Program
    {
        static bool Debug
        {
            get
            {
                bool isDebug = false;
                string sDebug = ConfigurationManager.AppSettings["debug"];

                Boolean.TryParse(sDebug, out isDebug);

                return isDebug;
            }
        }

        static string ldapPath = null;

        static void Main(string[] args)
        {
            string username, password;

            username = null;
            password = null;

            Console.WriteLine("Active Directory List version 0.1");

            List<Core.Users> lstADUsers = null;
            List<Core.Users> lstADUsersDiscarded = null;


            ldapPath = GetLdapPathFromArguments(args);

            if (Debug)
            {
                Console.WriteLine("OK 1 - GetLdapPathFromArguments");
                Console.WriteLine(string.Empty);
            }

            username = GetUsernameFromArguments(args);

            if (Debug)
            {
                Console.WriteLine("OK 2 - GetUsernameFromArguments");
                Console.WriteLine(string.Empty);
            }

            password = GetPasswordFromArguments(args);

            if (Debug)
            {
                Console.WriteLine("OK 3 - GetPasswordFromArguments");
                Console.WriteLine(string.Empty);
            }

            if (string.IsNullOrEmpty(ldapPath))
            {
                Console.WriteLine("Nenhum endereço LDAP informado");
                Console.WriteLine(string.Empty);
                Console.WriteLine("Digite um endereço no seguinte formato: LDAP://dominio [usuario] [senha]");
                Console.WriteLine(string.Empty);
                Console.WriteLine("Exemplos de uso:");
                Console.WriteLine("  LDAP://dominio");
                Console.WriteLine("  LDAP://endereco-ip/dc=example,dc=com");
                Console.WriteLine("  LDAP://servidor/ou=users,dc=example,dc=com");
                Console.WriteLine("  LDAP://dominio/uid=username,dc=example,dc=com");
                return;
            }

            /*
            if (string.IsNullOrEmpty(ldapPath) || (args.Length > 0 && (args[0] == "-h" || args[0] == "/?")))
            {
                if (string.IsNullOrEmpty(ldapPath))
                {
                    Console.WriteLine("Nenhum endereço LDAP informado");
                }
                Console.WriteLine(string.Empty);
                Console.WriteLine("Digite um endereço no seguinte formato: LDAP://dominio [usuario] [senha]");
                return;
            }
            */

            Core.LdapList ldap = new Core.LdapList();

            if (Debug)
            {
                Console.WriteLine("OK 5 - LdapList");
                Console.WriteLine(string.Empty);
            }

            try
            {
                lstADUsers = ldap.GetADUsersWrapper(ldapPath, ref lstADUsersDiscarded, username, password);

                if (Debug)
                {
                    Console.WriteLine("OK 6 - GetADUsersWrapper");
                    Console.WriteLine(string.Empty);
                }
            }
            catch (Exception e)
            {
                if (Debug)
                {
                    Console.WriteLine("OK 7");
                    Console.WriteLine(string.Empty);
                }

                Console.WriteLine("Ocorreu um erro ao consultar o LDAP " + ldapPath + ".");
                Console.WriteLine("Mensagem: " + e.Message);
                Console.WriteLine("Rastreamento: " + e.StackTrace);
                Console.WriteLine(string.Empty);
            }

            if (Debug)
            {
                Console.WriteLine("OK 8");
                Console.WriteLine(string.Empty);
            }

            if (lstADUsers != null)
            {

                if (Debug)
                {
                    Console.WriteLine("OK 9");
                    Console.WriteLine(string.Empty);
                }

                if (lstADUsers.Count > 0 || lstADUsersDiscarded.Count > 0)
                {
                    Console.WriteLine(string.Empty);
                    Console.WriteLine("LDAP consultado. " + lstADUsers.Count.ToString() + " registros retornados.");
                    Console.WriteLine("" + lstADUsersDiscarded.Count.ToString() + " registros descartados.");
                    Console.WriteLine(string.Empty);

                    PrintResults("Usuarios obtidos", lstADUsers);
                    Console.WriteLine(string.Empty);
                    PrintResults("Itens descartados", lstADUsersDiscarded);
                    Console.WriteLine(string.Empty);
                }
                else
                {
                    Console.WriteLine(string.Empty);
                    Console.WriteLine("LDAP consultado. " + lstADUsers.Count.ToString() + " registros retornados.");
                    Console.WriteLine("" + lstADUsersDiscarded.Count.ToString() + " registros descartados.");
                }
            }
            else
            {
                Console.WriteLine(string.Empty);
                Console.WriteLine("LDAP consultado. 0 registros retornados.");
            }

            lstADUsers = null;

            ldap = null;

        }

        static string GetLdapPathFromArguments(string[] args)
        {
            string transformedArgument = string.Empty;

            if (args.Length > 0)
            {
                if (Debug)
                {
                    Console.WriteLine();
                    Console.WriteLine("Parametros:");
                    for (int i = 0; i < args.Length; i++)
                    {
                        Console.WriteLine(i.ToString() + " " + args[i]);
                    }
                }

                if (!string.IsNullOrEmpty(args[0]))
                {
                    if (args[0].ToString().ToLower().IndexOf("ldap://") == -1)
                    {
                        transformedArgument = "LDAP://" + args[0].ToString();
                    }
                    else
                    {
                        transformedArgument = args[0].ToString();
                        transformedArgument = transformedArgument.Replace("ldap://", "LDAP://");
                    }
                }
            }

            return transformedArgument;
        }

        static string GetUsernameFromArguments(string[] args)
        {
            string username = string.Empty;

            if (args.Length > 1)
            {
                username = args[1];
            }

            return username;
        }

        static string GetPasswordFromArguments(string[] args)
        {
            string password = string.Empty;

            if (args.Length > 2)
            {
                password = args[2];
            }

            return password;
        }

        static void PrintResults(string HeaderText, List<Core.Users> resultList)
        {
            Console.WriteLine(HeaderText);

            foreach (Core.Users adUser in resultList)
            {
                bool isNormalAccount = (adUser.UserAccountControl & UserAccountControl.NORMAL_ACCOUNT) == UserAccountControl.NORMAL_ACCOUNT;
                bool isAccountDisabled = (adUser.UserAccountControl & UserAccountControl.ACCOUNTDISABLE) == UserAccountControl.ACCOUNTDISABLE;
                bool isAccountLockedOut = (adUser.UserAccountControl & UserAccountControl.LOCKOUT) == UserAccountControl.LOCKOUT;

                if (!isAccountDisabled)
                {
                    Console.Write("DisplayName: ");
                    Console.Write(adUser.DisplayName);
                    Console.Write(" Email: ");
                    Console.Write(adUser.Email);
                    Console.Write(" UserName: ");
                    Console.WriteLine(adUser.UserName);
                    Console.Write(" Name: ");
                    Console.WriteLine(adUser.Name);
                    Console.Write(" GivenName: ");
                    Console.WriteLine(adUser.GivenName);
                    Console.Write(" UserPrincipalName: ");
                    Console.WriteLine(adUser.UserPrincipalName);
                    Console.Write(" CanonicalName: ");
                    Console.WriteLine(adUser.CanonicalName);
                    Console.Write(" Uid: ");
                    Console.WriteLine(adUser.Uid);
                    Console.Write(" DistinguishedName: ");
                    Console.WriteLine(adUser.DistinguishedName);
                }
                else
                {
                    Console.Write("USUÁRIO DESABILITADO ");
                    Console.Write(" DisplayName: ");
                    Console.Write(adUser.DisplayName);
                    Console.Write(" Email: ");
                    Console.Write(adUser.Email);
                    Console.Write(" UserName: ");
                    Console.WriteLine(adUser.UserName);
                    Console.Write(" Name: ");
                    Console.WriteLine(adUser.Name);
                    Console.Write(" GivenName: ");
                    Console.WriteLine(adUser.GivenName);
                    Console.Write(" UserPrincipalName: ");
                    Console.WriteLine(adUser.UserPrincipalName);
                    Console.Write(" CanonicalName: ");
                    Console.WriteLine(adUser.CanonicalName);
                    Console.Write(" Uid: ");
                    Console.WriteLine(adUser.Uid);
                    Console.Write(" DistinguishedName: ");
                    Console.WriteLine(adUser.DistinguishedName);
                }
            }
        }

    }
}
