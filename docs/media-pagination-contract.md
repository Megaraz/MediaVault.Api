# Media collection pagination contract

`GET /mediaentries` and `POST /mediaentries/search` return the same successful
JSON envelope:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 25,
  "totalCount": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

The API normalizes page numbers to a minimum of 1 and page sizes to the range
1-100. `totalPages` is zero for an empty result and otherwise uses ceiling
division. Continuation flags are derived from those normalized values and the
total count; clients must not infer continuation from `items.length`.

The collection query counts only the authenticated owner's entries. Search
uses that same owner boundary plus its title filter for both count and items.
The count and page query execute in one short SQLite read transaction, so one
response describes one database snapshot. Items retain deterministic ordering
by `CreatedAtUtc` descending and then `Id` ascending.

The envelope does not create a snapshot across separate HTTP requests. Writes
between page requests can change counts or move entries between pages; clients
must use each response's current metadata and replace stale page results.
