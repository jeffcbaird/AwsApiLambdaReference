STAGE   ?= dev
REGION  ?= us-west-2

ARTIFACT_DIR := artifacts
ARTIFACT     := $(ARTIFACT_DIR)/LambdaApiReference.zip
PROJECT_DIR  := src/LambdaApiReference
TEST_DIR     := test/LambdaApiReference.Tests

.DEFAULT_GOAL := help
.PHONY: help restore build test clean

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
	  | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-16s\033[0m %s\n", $$1, $$2}'

restore: ## Restore NuGet packages and dotnet tools
	dotnet tool restore
	dotnet restore

build: restore ## Package Lambda zip (output: artifacts/LambdaApiReference.zip)
	mkdir -p $(ARTIFACT_DIR)
	cd $(PROJECT_DIR) && dotnet lambda package \
	  --configuration Release \
	  --output-package ../../$(ARTIFACT) \
	  --function-architecture x86_64

test: ## Run all unit tests
	dotnet test $(TEST_DIR) --verbosity normal

clean: ## Remove build artifacts and bin/obj folders
	rm -rf $(ARTIFACT_DIR)
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true
