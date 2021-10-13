$wslIp=(wsl -d Ubuntu-20.04 -e sh -c "ip addr show eth0 | grep 'inet\b' | awk '{print `$2}' | cut -d/ -f1") # Get the private IP of the WSL2 instance

# MySql
echo "Forwarding port to MySql"
netsh interface portproxy delete v4tov4 listenport="3306" # Delete any existing port 3306 forwarding
netsh interface portproxy add v4tov4 listenport="3306" connectaddress="$wslIp" connectport="3306"

# adminer
echo "Forwarding port to MySql Adminer"
netsh interface portproxy delete v4tov4 listenport="8080" # Delete any existing port 8080 forwarding
netsh interface portproxy add v4tov4 listenport="8080" connectaddress="$wslIp" connectport="8080"

# oracle
echo "Forwarding ports to Oracle"
netsh interface portproxy delete v4tov4 listenport="51521" # Delete any existing port 51521 forwarding
netsh interface portproxy delete v4tov4 listenport="1521" # Delete any existing port 1521 forwarding
netsh interface portproxy add v4tov4 listenport="51521" connectaddress="$wslIp" connectport="51521"
netsh interface portproxy add v4tov4 listenport="1521" connectaddress="$wslIp" connectport="1521"

# sqlserver
echo "Forwarding port to SqlServer"
netsh interface portproxy delete v4tov4 listenport="1433" # Delete any existing port 1433 forwarding
netsh interface portproxy add v4tov4 listenport="1433" connectaddress="$wslIp" connectport="1433"

# db2
echo "Forwarding ports to DB2"
netsh interface portproxy delete v4tov4 listenport="50000" # Delete any existing port 50000 forwarding
netsh interface portproxy delete v4tov4 listenport="55001" # Delete any existing port 55001 forwarding
netsh interface portproxy add v4tov4 listenport="50000" connectaddress="$wslIp" connectport="50000"
netsh interface portproxy add v4tov4 listenport="55001" connectaddress="$wslIp" connectport="55001"

# postgresql
# netsh interface portproxy delete v4tov4 listenport="1433" # Delete any existing port 1433 forwarding
# netsh interface portproxy add v4tov4 listenport="1433" connectaddress="$wslIp" connectport="1433"

wsl -u root -- service docker start