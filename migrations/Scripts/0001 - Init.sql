create table raw_files(
    id serial primary key,
    name varchar(255) not null,
    path varchar(255) not null unique,
    conversion_status varchar(255) not null,
    metadata jsonb null
);

create table web_videos(
    id serial primary key,
    name varchar(255) not null,
    link varchar(255) not null unique,
    raw_file_id int not null unique,
    metadata jsonb null,
    constraint fk_web_videos_raw_files
        foreign key (raw_file_id)
        references raw_files(id)
        on delete cascade
);

create table web_video_subtitles(
    id serial primary key,
    web_video_id int not null references web_videos(id),
    language varchar(255) not null,
    link varchar(255) not null unique,
    metadata jsonb null
);