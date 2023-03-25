create table transactions(
    id uuid primary key default gen_random_uuid(),
    date timestamp not null default now(),
    type varchar(50) not null,
    btc_amount decimal not null,
    fiat_amount decimal,
    fiat_code text,
    created timestamp not null default now(),
    updated timestamp not null default now()
);

create table user_transactions(
    id uuid primary key default gen_random_uuid(),
    transaction_id uuid not null,
    user_id text not null,
    created timestamp not null default now(),
    updated timestamp not null default now(),
    constraint fk_transaction_id
        foreign key (transaction_id)
            references transactions(id),
    constraint fk_user_id
        foreign key (user_id)
            references "AspNetUsers"("Id")
);