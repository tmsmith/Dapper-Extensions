The YAML files provided allow to load a docker environment for testing the databases of DapperExtensions.

It's fully compatible with Windows WSL2.

If you don't want the monitoring tools comment out or exclude containers "prometheus", "node-exporter", "cadvisor" and "grafana".

This files are using MariaDB as MySQL database.

For SQL Server and Oracle is necessary to create users to connect or change connectionstrings.json to use system users