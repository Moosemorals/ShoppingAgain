
SHELL := bash
.SHELLFLAGS := -eu -o pipefail -c
.ONESHELL:
.DELETE_ON_ERROR:
MAKEFLAGS += --warn-undefined-variables --no-builtin-rules
.RECIPEPREFIX = >

deploy:
> if ! git diff-index --quiet HEAD --; then 
>   echo "Won't publish with uncommited changes"
>   exit
> fi
> dotnet publish --output target/Shopping --configuration Release --runtime linux-x64
> tar -czf target/Shopping.tgz -C target Shopping
> #scp target/Shopping.tgz shopping@nuit:/home/shopping
> #ssh shopping@nuit /home/shopping/bin/deploy.sh
.PHONY: build

clean:
> -rm -rf target
