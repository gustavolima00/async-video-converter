create table webhook_users(
    id serial primary key,
    uuid uuid not null unique,
    webhook_url varchar(255) not null,
    events varchar(100)[] not null
);

create table raw_videos(
    id serial primary key,
    name varchar(255) not null,
    path varchar(255) not null,
    extract_subtitle_status varchar(100) not null,
    extract_tracks_status varchar(100) not null,
    user_uuid uuid,

    constraint fk_raw_videos_webhook_users
        foreign key (user_uuid)
        references webhook_users(uuid)
        on delete set null
);

create table raw_subtitles(
    id serial primary key,
    language varchar(255),
    path varchar(255) not null,
    raw_video_id int not null,

    constraint fk_videos_raw_videos
        foreign key (raw_video_id)
        references raw_videos(id)
        on delete cascade
);

create table converted_videos(
    id serial primary key,
    raw_video_id int not null unique,
    constraint fk_web_videos_videos
        foreign key (raw_video_id)
        references raw_videos(id)
        on delete cascade
);

create table converted_video_tracks(
    id serial primary key,
    converted_video_id int not null,
    path varchar(255) not null,
    language varchar(255) not null,
    link varchar(255) not null unique,
    constraint fk_converted_video_tracks_converted_videos
        foreign key (converted_video_id)
        references converted_videos(id)
        on delete cascade
);

create table converted_subtitles(
    id serial primary key,
    converted_video_id int not null,
    raw_subtitle_id int null unique,
    path varchar (255) not null,
    language varchar(255) not null,
    link varchar(255) not null unique,
    constraint fk_converted_subtitles_converted_videos
        foreign key (converted_video_id)
        references converted_videos(id)
        on delete cascade,

    constraint fk_converted_video_subtitles_raw_subtitles
        foreign key (raw_subtitle_id)
        references raw_subtitles(id)
        on delete cascade
);

-- Webhook users
create index idx_webhook_users_uuid on webhook_users(uuid);

-- Raw videos
create index idx_raw_videos_path on raw_videos(path);
create index idx_raw_videos_user_uuid on raw_videos(user_uuid);