create table raw_file(
    id serial primary key,
    name varchar(255) not null,
    path varchar(255) not null,
    metadata jsonb null,
);

create index raw_file on raw_file(path);