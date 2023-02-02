CREATE TABLE IF NOT EXISTS public.pathfinder
(
    pathfinder_id uuid NOT NULL DEFAULT uuid_generate_v1mc(),
    first_name text NOT NULL,
    last_name text NOT NULL,
    email text NOT NULL,
    create_timestamp timestamp with time zone NOT NULL,
    update_timestamp timestamp with time zone,
    grade integer,
    club_id uuid NOT NULL,
    CONSTRAINT "User_pkey" PRIMARY KEY (pathfinder_id),
    CONSTRAINT email_unique UNIQUE (email)
        INCLUDE(email),
    CONSTRAINT club_id_fkey FOREIGN KEY (club_id)
        REFERENCES public.club (club_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

ALTER TABLE IF EXISTS public.pathfinder
    OWNER to postgres;

CREATE INDEX IF NOT EXISTS fki_club_id_fkey
    ON public.pathfinder USING btree
    (club_id ASC NULLS LAST);

CREATE TRIGGER trigger_set_created_timestamp
    BEFORE INSERT
    ON public.pathfinder
    FOR EACH ROW
    EXECUTE FUNCTION public.trigger_set_created_timestamp();

CREATE TRIGGER trigger_set_updated_timestamp
    BEFORE INSERT OR UPDATE 
    ON public.pathfinder
    FOR EACH ROW
    EXECUTE FUNCTION public.trigger_set_updated_timestamp();