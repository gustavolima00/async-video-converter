create table webhook_user(
    id serial primary key,
    uuid uuid not null unique,
    webhook_url varchar(255) not null
);

create table raw_files(
    id serial primary key,
    name varchar(255) not null,
    path varchar(255) not null unique,
    conversion_status varchar(255) not null,
    user_uuid uuid,
    metadata jsonb,

    constraint fk_raw_files_webhook_user
        foreign key (user_uuid)
        references webhook_user(uuid)
        on delete set null
);

create table web_videos(
    id serial primary key,
    name varchar(255) not null,
    path varchar(255) not null,
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
    web_video_id int not null,
    path varchar (255) not null,
    language varchar(255) not null,
    link varchar(255) not null unique,
    metadata jsonb null,
    constraint fk_web_video_subtitles_web_videos
        foreign key (web_video_id)
        references web_videos(id)
        on delete cascade
);
