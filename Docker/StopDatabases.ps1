[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [ValidateSet($true, $false)]
    [boolean]$Shutdown = $false
)

wsl -u root docker container stop docker_db2_1
wsl -u root docker container stop docker_mariadb_1
wsl -u root docker container stop docker_oracle-db_1
wsl -u root docker container stop docker_sqlserver_1
wsl -u root docker container stop docker_postgres_1
wsl -u root docker container stop docker_adminer_1

wsl -u root -- service docker stop

if ($Shutdown)
{
    wsl --shutdown
}
