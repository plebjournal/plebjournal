-- AspNetUsers
create table "AspNetUsers"
(
    "Id"                   text    not null
        constraint "PK_AspNetUsers"
            primary key,
    "UserName"             varchar(256),
    "NormalizedUserName"   varchar(256),
    "Email"                varchar(256),
    "NormalizedEmail"      varchar(256),
    "EmailConfirmed"       boolean not null,
    "PasswordHash"         text,
    "SecurityStamp"        text,
    "ConcurrencyStamp"     text,
    "PhoneNumber"          text,
    "PhoneNumberConfirmed" boolean not null,
    "TwoFactorEnabled"     boolean not null,
    "LockoutEnd"           timestamp with time zone,
    "LockoutEnabled"       boolean not null,
    "AccessFailedCount"    integer not null
);

create index "EmailIndex"
    on "AspNetUsers" ("NormalizedEmail");

create unique index "UserNameIndex"
    on "AspNetUsers" ("NormalizedUserName");


-- auto-generated definition
create table "AspNetUserTokens"
(
    "UserId"        text not null
        constraint "FK_AspNetUserTokens_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "LoginProvider" text not null,
    "Name"          text not null,
    "Value"         text,
    constraint "PK_AspNetUserTokens"
        primary key ("UserId", "LoginProvider", "Name")
);

-- auto-generated definition
create table "AspNetRoles"
(
    "Id"               text not null
        constraint "PK_AspNetRoles"
            primary key,
    "Name"             varchar(256),
    "NormalizedName"   varchar(256),
    "ConcurrencyStamp" text
);

create unique index "RoleNameIndex"
    on "AspNetRoles" ("NormalizedName");

-- auto-generated definition
create table "AspNetUserRoles"
(
    "UserId" text not null
        constraint "FK_AspNetUserRoles_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "RoleId" text not null
        constraint "FK_AspNetUserRoles_AspNetRoles_RoleId"
            references "AspNetRoles"
            on delete cascade,
    constraint "PK_AspNetUserRoles"
        primary key ("UserId", "RoleId")
);

create index "IX_AspNetUserRoles_RoleId"
    on "AspNetUserRoles" ("RoleId");

-- auto-generated definition
create table "AspNetUserLogins"
(
    "LoginProvider"       text not null,
    "ProviderKey"         text not null,
    "ProviderDisplayName" text,
    "UserId"              text not null
        constraint "FK_AspNetUserLogins_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    constraint "PK_AspNetUserLogins"
        primary key ("LoginProvider", "ProviderKey")
);

create index "IX_AspNetUserLogins_UserId"
    on "AspNetUserLogins" ("UserId");

-- auto-generated definition
create table "AspNetUserClaims"
(
    "Id"         integer generated by default as identity
        constraint "PK_AspNetUserClaims"
            primary key,
    "UserId"     text not null
        constraint "FK_AspNetUserClaims_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "ClaimType"  text,
    "ClaimValue" text
);

create index "IX_AspNetUserClaims_UserId"
    on "AspNetUserClaims" ("UserId");

-- auto-generated definition
create table "AspNetRoleClaims"
(
    "Id"         integer generated by default as identity
        constraint "PK_AspNetRoleClaims"
            primary key,
    "RoleId"     text not null
        constraint "FK_AspNetRoleClaims_AspNetRoles_RoleId"
            references "AspNetRoles"
            on delete cascade,
    "ClaimType"  text,
    "ClaimValue" text
);

create index "IX_AspNetRoleClaims_RoleId"
    on "AspNetRoleClaims" ("RoleId");