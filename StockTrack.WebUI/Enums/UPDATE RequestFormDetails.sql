UPDATE RequestFormDetails
SET IsDeleted = 1
WHERE Id IN (14, 15, 16, 17, 19, 21, 23, 24); -- Not: Silmek istediğiniz kayıtların Id'lerini buraya yazın.

SELECT 
    rfd.Id, 
    rfd.RequestFormId, 
    rfd.StatusId, 
    rfd.IsDeleted, 
    rf.RequestFormTypeId,
    rfd.CreatedDate
FROM RequestFormDetails rfd
INNER JOIN RequestForms rf ON rfd.RequestFormId = rf.Id
WHERE rf.RequestFormTypeId = 1 -- Not: Eğer Kargo'nun Enum değeri 2 değilse, sistemindeki değer neyse onu yaz.
  AND rfd.IsDeleted = 0;

