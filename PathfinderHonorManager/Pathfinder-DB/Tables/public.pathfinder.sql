CREATE TABLE public.pathfinder (
    pathfinder_id uuid DEFAULT public.uuid_generate_v1mc() NOT NULL,
    first_name text NOT NULL,
    last_name text NOT NULL,
    email text NOT NULL,
    create_timestamp timestamp with time zone NOT NULL,
    update_timestamp timestamp with time zone,
    grade integer
);
ALTER TABLE ONLY public.pathfinder
    ADD CONSTRAINT "User_pkey" PRIMARY KEY (pathfinder_id);
ALTER TABLE ONLY public.pathfinder
    ADD CONSTRAINT email_unique UNIQUE (email) INCLUDE (email);
CREATE TRIGGER trigger_set_created_timestamp BEFORE INSERT ON public.pathfinder FOR EACH ROW EXECUTE FUNCTION public.trigger_set_created_timestamp();
CREATE TRIGGER trigger_set_updated_timestamp BEFORE INSERT OR UPDATE ON public.pathfinder FOR EACH ROW EXECUTE FUNCTION public.trigger_set_updated_timestamp();
