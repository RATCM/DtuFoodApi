# DTU Food Trucks
## Setting up development container
The development container currently only contains
a postgresql database. To set it up, make sure you
have docker installed and running, and then run the
following command in the terminal from the root of the
application
```shell

docker compose -f compose.dev.yml up -d
```

### Removing containers
If you want to remove the containers and their volumes
use the following command
```shell

docker compose -f compose.dev.yml down -v
```

## User secrets
This project uses user-secrets to hide certain credentials
like connection strings to the database, and token keys.

To list all the user secrets, run the following command
```shell

cd DtuFoodAPI

dotnet user-secrets list
```

### Connection string
To setup the connection string run the following command
from the project path in the terminal

```shell

cd DtuFoodAPI

dotnet user-secrets set "ConnectionStrings:Database" "host=localhost; port=5432; database=DtuFoodDb; username=dev; password=dev" 
```

This should only be used in development, and the connection string should be changed
if you don't use the database from the docker container.

### Jwt keys

To set the jwt keys in the user secrets, make sure you have python installed and run the following script
```shell

cd DtuFoodApi

dotnet user-secrets set "Authentication:Schemes:Access:SigningKeys:0:Value" $(python3 -c "import secrets; import base64; print(base64.b64encode(secrets.token_bytes(256)).decode('utf-8'))")
dotnet user-secrets set "Authentication:Schemes:Refresh:SigningKeys:0:Value" $(python3 -c "import secrets; import base64; print(base64.b64encode(secrets.token_bytes(256)).decode('utf-8'))")
```