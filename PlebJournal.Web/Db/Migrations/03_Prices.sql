create table prices (
    id uuid primary key default gen_random_uuid(),
    date timestamp not null default now(),
    price decimal not null,
    currency varchar(10) not null,
    created timestamp not null default now(),
    updated timestamp not null default now()
);

create index price_currency on prices (currency);
