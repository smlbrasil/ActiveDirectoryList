ActiveDirectoryList
-------------------
Software para conexão e pesquisa nos diretórios Active Directory ou LDAP.
Data: 01/10/2015


Exemplos de uso
---------------

PESQUISA GERAL LDAP
-------------------
AD
ActiveDirectoryList.exe LDAP://dominio

LDAP com usuario para pesquisa
ActiveDirectoryList.exe LDAP://endereco-ip/dc=example,dc=com cn=usuario,dc=example,dc=com senha
ActiveDirectoryList.exe LDAP://servidor/ou=users,dc=example,dc=com cn=usuario,dc=example,dc=com senha
ActiveDirectoryList.exe LDAP://dominio/uid=username,dc=example,dc=com cn=usuario,dc=example,dc=com senha

PESQUISAR POR OU
----------------
ActiveDirectoryList.exe LDAP://endereco-ip/ou=usuarios,dc=example,dc=com cn=usuario,dc=example,dc=com senha

Copyright Cryo Technologies 2015 - Todos os direitos reservados
