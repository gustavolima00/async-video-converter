create table raw_files(
    id serial primary key,
    name varchar(255) not null,
    path varchar(255) not null,
    metadata jsonb null
);

create index raw_files_path_idx on raw_files(name);