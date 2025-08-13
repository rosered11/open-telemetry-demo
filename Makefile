.PHONY: product
product:
	docker-compose build product
.PHONY: order
order:
	docker-compose build order

.PHONY: start
start:
	docker-compose up --force-recreate --remove-orphans --detach

.PHONY: stop
stop:
	docker-compose down --remove-orphans --volumes