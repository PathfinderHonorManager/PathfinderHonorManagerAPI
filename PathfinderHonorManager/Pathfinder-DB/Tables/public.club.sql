CREATE TABLE IF NOT EXISTS public.club
(
    club_id uuid NOT NULL DEFAULT uuid_generate_v1mc(),
    club_code text COLLATE pg_catalog."default" NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT club_pkey PRIMARY KEY (club_id),
    CONSTRAINT club_uid UNIQUE (club_code)
)
