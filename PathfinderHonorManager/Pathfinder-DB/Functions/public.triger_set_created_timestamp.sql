CREATE FUNCTION public.trigger_set_created_timestamp() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  NEW.create_timestamp = NOW();
  RETURN NEW;
END;
$$;