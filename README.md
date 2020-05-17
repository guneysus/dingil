# dingil

- https://gist.github.com/guneysus/e20508dcf3e0bf3bd8ebfbbeec09b912/raw/catalog.yml
- https://gist.github.com/guneysus/e20508dcf3e0bf3bd8ebfbbeec09b912/raw/todo.yml
- https://www.myget.org/feed/guneysu/package/nuget/Dingoz

```powershell
dotnet tool install -g Dingoz --version 1.0.0 --add-source https://www.myget.org/F/guneysu/api/v3/index.json

dingoz --url=https://gist.github.com/guneysus/e20508dcf3e0bf3bd8ebfbbeec09b912/raw/todo.yml --in-memory
```

```powershell
> http POST :5000/api/todo Title="Lorem ipsum"
```

```
HTTP/1.1 200 OK
Content-Type: application/json
Date: Tue, 24 Mar 2020 10:32:38 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
    "errors": [],
    "items": [
        {
            "Id": 2,
            "Title": "Lorem ipsum"
        }
    ]
}
```

```powershell
>  http :5000/api/todo
HTTP/1.1 200 OK
Content-Type: application/json
Date: Tue, 24 Mar 2020 10:33:08 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
    "errors": [],
    "items": [
        {
            "Id": 2,
            "Title": "Lorem ipsum"
        }
    ]
}
```


```powershell
>  http PUT :5000/api/todo/1 Title="Lorem Ipsum (DONE)"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Tue, 24 Mar 2020 10:35:22 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
    "errors": [],
    "result": true
}
```

```powershell
>  http  :5000/api/todo
HTTP/1.1 200 OK
Content-Type: application/json
Date: Tue, 24 Mar 2020 10:36:14 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
    "errors": [],
    "items": [
        {
            "Id": 1,
            "Title": "Lorem Ipsum (DONE)"
        }
    ]
}
```
