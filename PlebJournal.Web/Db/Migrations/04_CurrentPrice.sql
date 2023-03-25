create table current_prices (
    id uuid primary key default gen_random_uuid(),
    price decimal not null,
    currency varchar(10) not null unique,
    created timestamp not null default now(),
    updated timestamp not null default now()
);

create index current_price_currency on current_prices (currency);